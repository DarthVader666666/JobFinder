import { ref } from "vue";

const currentVersion = ref(null);

async function loadVersion() {
  const res = await fetch("../dist/version.json", { cache: "no-store" });
  const json = await res.json();
  return json.version;
}

async function startChecking(onUpdateDetected) {
  currentVersion.value = await loadVersion();
  setInterval(async () => {
    const serverVersion = await loadVersion();
    if (serverVersion !== currentVersion.value) {
      onUpdateDetected();
    }
  }, 30000);
}

export const versionChecker = {
  startChecking,
};
