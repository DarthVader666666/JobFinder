<script setup>
import "@/assets/main.css";
import SearchBar from "./components/SearchBar.vue";
import Button from "primevue/button";
import Toast from "primevue/toast";
import PendingModal from "./components/Modals/PendingModal.vue";
import SearchBarModal from "./components/Modals/SearchBarModal.vue";
import SettingsModal from "./components/Modals/SettingsModal.vue";
import SourcesComponent from "./components/SourcesComponent.vue";
import FilterComponent from "./components/FilterComponent.vue";
import JobItem from "./components/JobItem.vue";
import { computed, onMounted, ref } from "vue";
import { useStore } from "vuex";
import { helper } from "./helper.js";

const store = useStore();
const isPending = computed(() => store.getters.getPending);
const jobs = computed(() => store.getters.getFilteredJobs);
const isJobsEmpty = computed(() => jobs.value.length === 0);
const usdRate = ref(null);
const eurRate = ref(null);
const rubRate = ref(null);

onMounted(async () => {
  await helper.updateCurrencyRates();
  usdRate.value = store.getters.getUsdRate;
  eurRate.value = store.getters.getEurRate;
  rubRate.value = store.getters.getRubRate;
});

const showSearchBarModal = computed({
  get: () => store.getters.getShowSearchBarModal,
  set: (value) => store.commit("setShowSearchBarModal", value),
});

const showSettingsModal = computed({
  get: () => store.getters.getShowSettingsModal,
  set: (value) => store.commit("setShowSettingsModal", value),
});
</script>

<template>
  <Toast style="width: 320px" />
  <div class="header">
    <div class="title">
      <span style="font-size: 2rem">Find Your Job</span>
      <span>Поиск работы в РБ</span>
    </div>
    <div class="rates">
      <span>USD: {{ usdRate }}</span>
      <span>EUR: {{ eurRate }}</span>
      <span>RUB: {{ Math.round(rubRate * 100) / 10000 }}</span>
    </div>
  </div>
  <div class="main">
    <div class="settings" :class="{ mobileVisible: isJobsEmpty }">
      <SearchBar></SearchBar>
      <div class="sources-and-filter">
        <span>Источники</span>
        <hr />
        <SourcesComponent></SourcesComponent>
        <hr />
        <FilterComponent></FilterComponent>
      </div>
    </div>
    <div class="job-list" :class="{ mobileVisible: isJobsEmpty }">
      <div v-for="(job, index) in jobs" :key="index">
        <JobItem :job="job"></JobItem>
      </div>
    </div>
  </div>
  <div class="buttons" :class="{ mobileVisible: isJobsEmpty }">
    <Button rounded @click="store.commit('setShowSearchBarModal', true)"
      ><i class="pi pi-search"></i
    ></Button>
    <Button rounded @click="store.commit('setShowSettingsModal', true)"
      ><i class="pi pi-sliders-h"></i
    ></Button>
  </div>
  <SearchBarModal v-model:visible="showSearchBarModal"></SearchBarModal>
  <SettingsModal v-model:visible="showSettingsModal"></SettingsModal>
  <PendingModal v-model:visible="isPending"></PendingModal>
</template>

<style scoped>
.header {
  display: flex;
  justify-content: space-between;
}

.title {
  display: flex;
  flex-direction: column;
  gap: 5px;
  padding-left: 20px;
  padding-bottom: 10px;
  color: rgb(16, 185, 129);
  text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.7);
}

.rates {
  display: flex;
  flex-direction: column;
  padding-left: 20px;
  color: rgb(230, 230, 230);
}

.main {
  padding: 20px;
  display: flex;
  flex-direction: row;
  gap: 20px;
}

.settings {
  position: sticky;
  top: 10px;
  align-self: flex-start;
  min-width: 180px;
  width: 25%;
}

.job-list {
  width: 70%;
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.buttons {
  display: none;
  position: fixed;
  bottom: 30px;
  right: 30px;
  opacity: 0.7;
  z-index: 1;

  button {
    width: 60px;
    height: 60px;

    i {
      font-size: 1.4rem;
    }
  }
}

.sources-and-filter {
  padding: 15px;
  text-align: center;
  background: white;
  border-radius: 10px;
}

@media (max-width: 500px) {
  .main {
    padding: 0px;
  }

  .job-list {
    width: 100%;
  }

  .settings {
    display: none;
  }

  .settings.mobileVisible {
    width: 100%;
    display: block;
  }

  .buttons {
    display: flex;
    flex-direction: column;
    gap: 15px;
  }

  .buttons.mobileVisible {
    display: none;
  }

  .job-list.mobileVisible {
    display: none;
  }
}
</style>
