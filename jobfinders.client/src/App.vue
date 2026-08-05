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
import InputText from "primevue/inputtext";
import { computed, onBeforeUnmount, onMounted, ref, watch } from "vue";
import { useStore } from "vuex";
import { helper } from "./helper.js";
import { useToast } from "primevue/usetoast";
import FeedbackModal from "./components/Modals/FeedbackModal.vue";

const toast = useToast();
const store = useStore();

const usdRate = ref(null);
const eurRate = ref(null);
const rubRate = ref(null);
const showFeedbackModal = ref(false);

const savedJobsShown = computed(() => store.getters.getSavedJobsShown);
const isPending = computed(() => store.getters.getPending);
const isSending = computed(() => store.getters.getSending);
const jobs = computed(() =>
  savedJobsShown.value
    ? store.getters.getSavedJobs
    : store.getters.getFilteredJobs,
);
const isJobsEmpty = computed(() => jobs.value.length === 0);
const savedJobs = computed(() => store.getters.getSavedJobs);

const showSearchBarModal = computed({
  get: () => store.getters.getShowSearchBarModal,
  set: (value) => store.commit("setShowSearchBarModal", value),
});

const showSettingsModal = computed({
  get: () => store.getters.getShowSettingsModal,
  set: (value) => store.commit("setShowSettingsModal", value),
});

const firstPage = ref(0);
const rows = 20;

const slicedJobs = computed(() =>
  jobs.value.slice(firstPage.value * rows, firstPage.value * rows + rows),
);

const isFirstPage = computed(() => firstPage.value <= 0);
const isLastPage = computed(
  () => firstPage.value * rows + slicedJobs.value.length >= jobs.value.length,
);

watch(savedJobs, (newValue) => {
  if (!newValue.length) {
    store.dispatch("showSavedJobs", false);
  }
});

onMounted(async () => {
  window.addEventListener("resize", updateIsMobile);

  await helper.updateCurrencyRates();

  usdRate.value = store.getters.getUsdRate;
  eurRate.value = store.getters.getEurRate;
  rubRate.value = store.getters.getRubRate;
  scrollUp();
});

onBeforeUnmount(() => {
  window.removeEventListener("resize", updateIsMobile);
});

function updateIsMobile() {
  if (window.innerWidth > 500) {
    if (showSearchBarModal.value) {
      store.commit("setShowSearchBarModal", false);
    }

    if (showSettingsModal.value) {
      store.commit("setShowSettingsModal", false);
    }
  }
}

function saveJob(job) {
  job.saved = !job.saved;

  if (job.saved) {
    store.dispatch("addSavedJob", job);
    store.dispatch("showSuccess", {
      toast: toast,
      summary: "Вакансия сохранена",
      detail: job.title,
    });
  } else {
    store.dispatch("removeSavedJob", job);
  }
}

function handleShowFeedbackModal(value) {
  showFeedbackModal.value = value;
}

function scrollUp() {
  window.scrollTo({ top: 0, behavior: "smooth" });
}

function navigationHandler(direction) {
  if (direction === "back" && firstPage.value > 0) {
    firstPage.value--;
    scrollUp();
  }

  if (
    direction === "forward" &&
    firstPage.value * rows + slicedJobs.value.length < jobs.value.length
  ) {
    firstPage.value++;
    scrollUp();
  }
}

function resetFirstPage() {
  firstPage.value = 0;
}
</script>

<template>
  <Toast style="width: 320px; z-index: 2" />
  <div class="header">
    <div class="title">
      <span style="font-size: 1.6rem">Find Your Job</span>
      <span style="font-size: 0.9rem">Поиск работы в РБ</span>
    </div>
    <div style="display: flex; align-items: center; gap: 10px">
      <Button
        :icon="`pi ${isSending ? 'pi-spinner pi-spin' : 'pi-send'}`"
        :disabled="isSending"
        rounded
        severity="secondary"
        title="Оставить отзыв"
        @click="handleShowFeedbackModal(true)"
      >
      </Button>
      <Button
        :style="
          savedJobsShown
            ? { background: 'white', opacity: 0.9 }
            : { background: 'lightgray', opacity: 0.6 }
        "
        rounded
        severity="secondary"
        icon="pi pi-bookmark"
        :label="`${savedJobs.length || ''}`"
        @click="store.dispatch('showSavedJobs')"
      ></Button>
      <div class="rates">
        <span>USD: {{ usdRate }}</span>
        <span>EUR: {{ eurRate }}</span>
        <span>RUB: {{ Math.round(rubRate * 100) / 10000 }}</span>
      </div>
    </div>
  </div>
  <div v-if="false" class="search-job">
    <InputText placeholder="Поиск по списку"></InputText>
  </div>
  <div class="main">
    <div class="settings" :class="{ mobileVisible: isJobsEmpty }">
      <SearchBar></SearchBar>
      <div class="sources-and-filter">
        <SourcesComponent
          :disableSources="savedJobsShown"
          @resetFirstPage="resetFirstPage"
        ></SourcesComponent>
        <hr />
        <FilterComponent
          :disableFilter="savedJobsShown"
          @resetFirstPage="resetFirstPage"
        ></FilterComponent>
      </div>
    </div>
    <div class="job-list" :class="{ mobileVisible: isJobsEmpty }">
      <div v-for="(job, index) in slicedJobs" :key="index">
        <JobItem :job="job" @saveJob="saveJob(job)"></JobItem>
      </div>
      <div v-if="jobs.length" class="navigation-buttons">
        <Button
          rounded
          icon="pi pi-arrow-left"
          :disabled="isFirstPage"
          @click="navigationHandler('back')"
        ></Button>
        <span
          >{{ firstPage * rows + 1 }} ...
          {{ firstPage * rows + slicedJobs.length }}</span
        >
        <Button
          rounded
          icon="pi pi-arrow-right"
          :disabled="isLastPage"
          @click="navigationHandler('forward')"
        ></Button>
      </div>
    </div>
  </div>
  <div class="settings-buttons" :class="{ mobileVisible: isJobsEmpty }">
    <Button rounded @click="scrollUp"><i class="pi pi-arrow-up"></i></Button>
    <Button rounded @click="store.commit('setShowSearchBarModal', true)"
      ><i class="pi pi-search"></i
    ></Button>
    <Button rounded @click="store.commit('setShowSettingsModal', true)"
      ><i class="pi pi-sliders-h"></i
    ></Button>
  </div>
  <SearchBarModal v-model:visible="showSearchBarModal"></SearchBarModal>
  <SettingsModal
    :disableModal="savedJobsShown"
    v-model:visible="showSettingsModal"
    :resetFirstPage="resetFirstPage"
  ></SettingsModal>
  <FeedbackModal
    v-model:visible="showFeedbackModal"
    :toast="toast"
    @handleShowFeedbackModal="handleShowFeedbackModal"
  ></FeedbackModal>
  <PendingModal v-model:visible="isPending"></PendingModal>
</template>

<style scoped>
.header {
  display: flex;
  justify-content: space-between;
  width: 100%;
  padding: 10px 15px 15px 15px;
  position: fixed;
  z-index: 1;
  top: 0;
  background: linear-gradient(
    to bottom,
    var(--BACKGROUND-COLOR) 0%,
    var(--BACKGROUND-COLOR) 0%,
    transparent 100%
  );

  :deep(.p-button-icon) {
    font-size: 1.3rem;
  }
}

.title {
  display: flex;
  flex-direction: column;
  gap: 5px;
  color: rgb(16, 185, 129);
  text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.7);
  width: 35%;
  min-width: 160px;
}

.rates {
  display: flex;
  flex-direction: column;
  padding-left: 20px;
  color: rgb(230, 230, 230);
}

.search-job {
  position: sticky;
  top: 10px;
  display: flex;
  gap: 10px;
  padding: 10px;
  justify-content: center;

  input {
    width: 300px;
    box-shadow: 1px 1px 10px rgba(0, 0, 0, 0.3);
  }
}

.main {
  padding: 86px 20px 20px 20px;
  display: flex;
  flex-direction: row;
  gap: 40px;
}

.settings {
  position: sticky;
  top: 94px;
  align-self: flex-start;
  min-width: 180px;
  width: 25%;
  min-width: 290px;
}

.job-list {
  padding-bottom: 230px;
  width: 70%;
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.settings-buttons {
  display: none;
  position: fixed;
  bottom: 20px;
  right: 20px;
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

.navigation-buttons {
  position: sticky;
  bottom: 15px;
  display: flex;
  gap: 10px;
  justify-content: center;
  align-items: center;
  opacity: 0.7;

  span {
    background-color: rgb(16 185 129);
    opacity: 0.8;
    color: white;
    padding: 5px;
    border-radius: 30px;
    width: 80px;
    text-align: center;
  }
}

.sources-and-filter {
  padding: 15px;
  text-align: center;
  background: white;
  border-radius: 10px;
}

@media (max-width: 600px) {
  .main {
    padding: 0px;
  }

  .job-list {
    padding-top: 70px;
    width: 100%;
  }

  .settings {
    display: none;
  }

  .rates {
    display: none;
  }

  .settings.mobileVisible {
    padding-top: 80px;
    width: 100%;
    display: block;
  }

  .settings-buttons {
    display: flex;
    flex-direction: column;
    gap: 15px;
  }

  .settings-buttons.mobileVisible {
    display: none;
  }

  .job-list.mobileVisible {
    display: none;
  }
}
</style>
