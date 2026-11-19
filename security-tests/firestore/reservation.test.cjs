const { test, before, after, beforeEach } = require("node:test");
const fs = require("node:fs");
const {
  initializeTestEnvironment,
  assertSucceeds,
  assertFails
} = require("@firebase/rules-unit-testing");
const {
  doc, getDoc, getDocs, collection, setDoc, query, where,
  deleteDoc, serverTimestamp, writeBatch
} = require("firebase/firestore");

let env;

before(async () => {
  env = await initializeTestEnvironment({
    projectId: "demo-altrafloat",
    firestore: {
      host: "127.0.0.1",
      port: 8080,
      rules: fs.readFileSync("firestore.rules", "utf8")
    }
  });
});

beforeEach(async () => {
  await env.clearFirestore();
});

after(async () => {
  await env.cleanup();
});

function dbFor(uid) {
  return env.authenticatedContext(uid).firestore();
}

async function seedUser(uid, data) {
  await env.withSecurityRulesDisabled(async (context) => {
    await setDoc(doc(context.firestore(), "users", uid), data);
  });
}

async function seedDirectory(uid, username, loId, status = "active") {
  await env.withSecurityRulesDisabled(async (context) => {
    await setDoc(doc(context.firestore(), "liquidityOfficerDirectory", uid), {
      username, loId, status
    });
  });
}

async function seedAgent(uid = "agent1") {
  await seedUser(uid, {
    uid, username: "Agent", email: `${uid}@gmail.com`,
    role: "agent", accountStatus: "active"
  });
}

async function seedLO(uid, status = "approved", accountStatus = "active") {
  await seedUser(uid, {
    uid, username: "Officer", email: `${uid}@gmail.com`,
    role: "liquidityOfficer",
    approvalStatus: status, accountStatus
  });
  const directoryStatus =
    status === "approved" && accountStatus === "active" ? "active" : "inactive";
  await seedDirectory(uid, "Officer", "S012345", directoryStatus);
}

test("Active Agent can read approved active LO directory entry", async () => {
  await seedAgent();
  await seedLO("lo1");
  await assertSucceeds(getDoc(doc(dbFor("agent1"), "liquidityOfficerDirectory", "lo1")));
});

test("Active Agent can list approved LO directory", async () => {
  await seedAgent();
  await seedLO("lo1");
  await assertSucceeds(getDocs(query(collection(dbFor("agent1"), "liquidityOfficerDirectory"), where("status", "==", "active"))));
});

test("Pending LO cannot be read from directory", async () => {
  await seedAgent();
  await seedLO("lo1", "pending", "pending");
  await assertFails(getDoc(doc(dbFor("agent1"), "liquidityOfficerDirectory", "lo1")));
});

test("Suspended LO cannot be read from directory", async () => {
  await seedAgent();
  await seedLO("lo1", "approved", "suspended");
  await assertFails(getDoc(doc(dbFor("agent1"), "liquidityOfficerDirectory", "lo1")));
});

test("Rejected LO cannot be read from directory", async () => {
  await seedAgent();
  await seedLO("lo1", "rejected", "active");
  await assertFails(getDoc(doc(dbFor("agent1"), "liquidityOfficerDirectory", "lo1")));
});

test("Stale directory entry without LO profile is denied", async () => {
  await seedAgent();
  await seedDirectory("missingLO", "Officer", "S012345");
  await assertFails(getDoc(doc(dbFor("agent1"), "liquidityOfficerDirectory", "missingLO")));
});

test("Non-Agent user cannot read directory", async () => {
  await seedLO("lo1");
  await assertFails(getDoc(doc(dbFor("lo1"), "liquidityOfficerDirectory", "lo1")));
});

test("Unauthenticated user cannot read directory", async () => {
  await seedLO("lo1");
  await assertFails(getDoc(doc(env.unauthenticatedContext().firestore(), "liquidityOfficerDirectory", "lo1")));
});

test("Agent cannot create directory entry", async () => {
  await seedAgent();
  await assertFails(setDoc(doc(dbFor("agent1"), "liquidityOfficerDirectory", "lo2"), {
    username: "Officer", loId: "S123456"
  }));
});

test("Agent cannot modify or delete directory entry", async () => {
  await seedAgent();
  await seedLO("lo1");
  const ref = doc(dbFor("agent1"), "liquidityOfficerDirectory", "lo1");
  await assertFails(setDoc(ref, { username: "Changed", loId: "S123456" }));
  await assertFails(deleteDoc(ref));
});

test("Active directory query excludes pending LO entry", async () => {
  await seedAgent();
  await seedLO("pendingLO", "pending", "pending");
  const result = await getDocs(query(collection(dbFor("agent1"), "liquidityOfficerDirectory"), where("status", "==", "active")));
  if (result.docs.some(d => d.id === "pendingLO")) {
    throw new Error("Pending LO entry was exposed.");
  }
});

async function attemptLORegistration(
  uid, email, loId, profileOverrides = {}, reservationOverrides = {}
) {
  const db = env.authenticatedContext(uid, { email }).firestore();
  const batch = writeBatch(db);

  batch.set(doc(db, "users", uid), {
    uid, username: "Test Officer", email,
    role: "liquidityOfficer",
    accountStatus: "pending",
    approvalStatus: "pending",
    createdAt: serverTimestamp(),
    loId,
    ...profileOverrides
  });

  batch.set(doc(db, "liquidityOfficerIds", loId), {
    uid, reserved: true, createdAt: serverTimestamp(),
    ...reservationOverrides
  });

  await batch.commit();
}

async function attemptAgentRegistration(
  uid, email, agentId, category = "Main Agent",
  profileOverrides = {}, reservationOverrides = {}
) {
  const db = env.authenticatedContext(uid, { email }).firestore();
  const batch = writeBatch(db);

  batch.set(doc(db, "users", uid), {
    uid, username: "Test Agent", email,
    role: "agent",
    accountStatus: "active",
    createdAt: serverTimestamp(),
    agentId,
    agentCategory: category,
    ...profileOverrides
  });

  batch.set(doc(db, "agentIds", agentId), {
    uid, reserved: true, createdAt: serverTimestamp(),
    ...reservationOverrides
  });

  await batch.commit();
}

test("Agent registration accepts a valid Agent profile", async () => {
  await assertSucceeds(
    attemptAgentRegistration("agent1", "agent@gmail.com", "G12345")
  );
});

test("Agent registration rejects lowercase Agent ID", async () => {
  await assertFails(
    attemptAgentRegistration("agent1", "agent1@gmail.com", "agent123")
  );
});

test("Agent registration rejects Agent ID starting with a digit", async () => {
  await assertFails(
    attemptAgentRegistration("agent1", "agent1@gmail.com", "123AGENT")
  );
});

test("Agent registration rejects Agent ID containing spaces", async () => {
  await assertFails(
    attemptAgentRegistration("agent1", "agent1@gmail.com", "AGENT 123")
  );
});

test("Agent registration rejects Agent ID with invalid punctuation", async () => {
  await assertFails(
    attemptAgentRegistration("agent1", "agent1@gmail.com", "AGENT@123")
  );
});

test("Agent registration accepts Main Agent category", async () => {
  await assertSucceeds(
    attemptAgentRegistration("agent1", "agent1@gmail.com", "G12345", "Main Agent")
  );
});

test("Agent registration accepts Moblie Money Agent category", async () => {
  await assertSucceeds(
    attemptAgentRegistration("agent2", "agent2@gmail.com", "G23456", "Moblie Money Agent")
  );
});

test("Agent registration accepts High Agents category", async () => {
  await assertSucceeds(
    attemptAgentRegistration("agent3", "agent3@gmail.com", "G34567", "High Agents")
  );
});

test("Agent registration rejects an unknown category", async () => {
  await assertFails(
    attemptAgentRegistration("agent1", "agent1@gmail.com", "G12345", "Unknown")
  );
});

test("Agent registration rejects an empty category", async () => {
  await assertFails(
    attemptAgentRegistration("agent1", "agent1@gmail.com", "G12345", "")
  );
});

test("Agent registration rejects incorrect category capitalization", async () => {
  await assertFails(
    attemptAgentRegistration("agent1", "agent1@gmail.com", "G12345", "main agent")
  );
});

test("LO registration accepts lowercase Gmail address", async () => {
  await assertSucceeds(
    attemptLORegistration("lo1", "officer@gmail.com", "S123456")
  );
});

test("LO registration rejects uppercase Gmail address", async () => {
  await assertFails(
    attemptLORegistration("lo1", "Officer@gmail.com", "S123456")
  );
});

test("LO registration rejects uppercase Gmail domain", async () => {
  await assertFails(
    attemptLORegistration("lo1", "officer@GMAIL.COM", "S123456")
  );
});

test("LO registration rejects non-Gmail address", async () => {
  await assertFails(
    attemptLORegistration("lo1", "officer@yahoo.com", "S123456")
  );
});

test("LO registration rejects trailing email spaces", async () => {
  await assertFails(
    attemptLORegistration("lo1", "officer@gmail.com ", "S123456")
  );
});

test("LO registration rejects misleading Gmail suffix", async () => {
  await assertFails(
    attemptLORegistration("lo1", "officer@gmail.com.evil", "S123456")
  );
});

test("Agent registration rejects an incorrect role", async () => {
  await assertFails(
    attemptAgentRegistration(
      "agent_wrong_role", "wrongrole@gmail.com", "G45671",
      "Main Agent", { role: "liquidityOfficer" }
    )
  );
});

test("Agent registration rejects an inactive account status", async () => {
  await assertFails(
    attemptAgentRegistration(
      "agent_inactive", "inactive@gmail.com", "G45672",
      "Main Agent", { accountStatus: "suspended" }
    )
  );
});

test("Agent registration rejects an empty username", async () => {
  await assertFails(
    attemptAgentRegistration(
      "agent_empty_name", "emptyname@gmail.com", "G45673",
      "Main Agent", { username: "" }
    )
  );
});

test("Agent registration rejects a profile UID that differs from the signed-in UID", async () => {
  await assertFails(
    attemptAgentRegistration(
      "agent_uid_mismatch", "uidcheck@gmail.com", "G45674",
      "Main Agent", { uid: "another_user" }
    )
  );
});

test("Agent registration rejects an email that differs from the authenticated email", async () => {
  await assertFails(
    attemptAgentRegistration(
      "agent_email_mismatch", "authcheck@gmail.com", "G45675",
      "Main Agent", { email: "different@gmail.com" }
    )
  );
});

test("Agent registration rejects a reservation owned by another UID", async () => {
  await assertFails(
    attemptAgentRegistration(
      "agent_res_owner", "resowner@gmail.com", "G45676",
      "Main Agent", {}, { uid: "another_user" }
    )
  );
});

test("Agent registration rejects a reservation with reserved set to false", async () => {
  await assertFails(
    attemptAgentRegistration(
      "agent_res_false", "resfalse@gmail.com", "G45677",
      "Main Agent", {}, { reserved: false }
    )
  );
});

test("LO registration rejects lowercase ID prefix", async () => {
  await assertFails(
    attemptLORegistration("lo_lower", "lower@gmail.com", "s123")
  );
});

test("LO registration rejects an ID with an invalid prefix", async () => {
  await assertFails(
    attemptLORegistration("lo_prefix", "prefix@gmail.com", "A123")
  );
});

test("LO registration rejects an ID without digits", async () => {
  await assertFails(
    attemptLORegistration("lo_nodigits", "nodigits@gmail.com", "S")
  );
});

test("LO registration rejects an ID containing spaces", async () => {
  await assertFails(
    attemptLORegistration("lo_spaces", "spaces@gmail.com", "S 123")
  );
});

test("LO registration rejects an incorrect role", async () => {
  await assertFails(
    attemptLORegistration(
      "lo_wrong_role", "wrongrolelo@gmail.com", "S45671",
      { role: "agent" }
    )
  );
});

test("LO registration rejects an account status other than pending", async () => {
  await assertFails(
    attemptLORegistration(
      "lo_wrong_account", "wrongaccount@gmail.com", "S45672",
      { accountStatus: "active" }
    )
  );
});

test("LO registration rejects an approval status other than pending", async () => {
  await assertFails(
    attemptLORegistration(
      "lo_wrong_approval", "wrongapproval@gmail.com", "S45673",
      { approvalStatus: "approved" }
    )
  );
});

test("LO registration rejects a reservation owned by another UID", async () => {
  await assertFails(
    attemptLORegistration(
      "lo_res_owner", "loresowner@gmail.com", "S45674",
      {}, { uid: "another_user" }
    )
  );
});

test("LO registration rejects a reservation with reserved set to false", async () => {
  await assertFails(
    attemptLORegistration(
      "lo_res_false", "loresfalse@gmail.com", "S45675",
      {}, { reserved: false }
    )
  );
});

test("Agent ID reservation cannot be read", async () => {
  await env.withSecurityRulesDisabled(async (context) => {
    await setDoc(doc(context.firestore(), "agentIds", "G45678"), {
      uid: "agent1", reserved: true
    });
  });

  await assertFails(
    getDoc(doc(dbFor("agent1"), "agentIds", "G45678"))
  );
});

test("Agent ID reservation cannot be updated", async () => {
  await env.withSecurityRulesDisabled(async (context) => {
    await setDoc(doc(context.firestore(), "agentIds", "G45679"), {
      uid: "agent1", reserved: true
    });
  });

  await assertFails(
    setDoc(doc(dbFor("agent1"), "agentIds", "G45679"), {
      uid: "agent1", reserved: false
    })
  );
});

test("Agent ID reservation cannot be deleted", async () => {
  await env.withSecurityRulesDisabled(async (context) => {
    await setDoc(doc(context.firestore(), "agentIds", "G45680"), {
      uid: "agent1", reserved: true
    });
  });

  await assertFails(
    deleteDoc(doc(dbFor("agent1"), "agentIds", "G45680"))
  );
});

test("LO ID reservation cannot be read", async () => {
  await env.withSecurityRulesDisabled(async (context) => {
    await setDoc(doc(context.firestore(), "liquidityOfficerIds", "S45678"), {
      uid: "lo1", reserved: true
    });
  });

  await assertFails(
    getDoc(doc(dbFor("lo1"), "liquidityOfficerIds", "S45678"))
  );
});

test("LO ID reservation cannot be updated", async () => {
  await env.withSecurityRulesDisabled(async (context) => {
    await setDoc(doc(context.firestore(), "liquidityOfficerIds", "S45679"), {
      uid: "lo1", reserved: true
    });
  });

  await assertFails(
    setDoc(doc(dbFor("lo1"), "liquidityOfficerIds", "S45679"), {
      uid: "lo1", reserved: false
    })
  );
});

test("LO ID reservation cannot be deleted", async () => {
  await env.withSecurityRulesDisabled(async (context) => {
    await setDoc(doc(context.firestore(), "liquidityOfficerIds", "S45680"), {
      uid: "lo1", reserved: true
    });
  });

  await assertFails(
    deleteDoc(doc(dbFor("lo1"), "liquidityOfficerIds", "S45680"))
  );
});
