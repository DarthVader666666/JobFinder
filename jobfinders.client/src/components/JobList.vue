<script setup>
import { computed } from "vue";
import JobItem from "./JobItem.vue";
import NavigationButtons from "./NavigationButtons.vue";
import { useStore } from "vuex";
import { helper } from "@/helper.js";

const store = useStore();

const firstPage = computed({
  get: () => store.getters.getFirstPage,
  set: (value) => store.commit("setFirstPage", value),
});

const props = defineProps({
  toast: {
    type: Object,
    default: null,
  },
  savedJobsShown: {
    type: Boolean,
    default: false,
  },
});

const rows = computed({
  get: () => store.getters.getRows,
  set: (value) => store.commit("setRows", value),
});

const jobs = computed(() =>
  props.savedJobsShown
    ? store.getters.getSavedJobsCache
    : store.getters.getFilteredJobs,
);

const slicedJobs = computed(() =>
  jobs.value.slice(
    firstPage.value * rows.value,
    firstPage.value * rows.value + rows.value,
  ),
);

const isFirstPage = computed(() => firstPage.value <= 0);
const isLastPage = computed(
  () =>
    firstPage.value * rows.value + slicedJobs.value.length >= jobs.value.length,
);

const jobsRange = computed(() => [
  firstPage.value * rows.value + 1,
  firstPage.value * rows.value + slicedJobs.value.length,
]);

function saveJob(job) {
  const index = slicedJobs.value.indexOf(job);
  var savedJob = slicedJobs.value[index];
  savedJob.saved = !savedJob.saved;

  if (savedJob.saved) {
    store.dispatch("addSavedJob", savedJob);
    store.dispatch("showSuccess", {
      toast: props.toast,
      summary: "Вакансия сохранена",
      detail: savedJob.title,
    });
  } else {
    store.dispatch("removeSavedJob", savedJob);
  }
}

function navigationHandler(direction) {
  if (direction === "back" && firstPage.value > 0) {
    firstPage.value--;
  }

  if (
    direction === "forward" &&
    firstPage.value * rows.value + slicedJobs.value.length < jobs.value.length
  ) {
    firstPage.value++;
  }

  helper.scrollUp();
}
</script>
<template>
  <div class="job-list">
    <div v-for="(job, index) in slicedJobs" :key="index">
      <JobItem :job="job" @saveJob="saveJob"></JobItem>
    </div>
    <NavigationButtons
      :isFirstPage="isFirstPage"
      :isLastPage="isLastPage"
      :jobsRange="jobsRange"
      :showNavigationButtons="slicedJobs.length > 0"
      @navigationHandler="navigationHandler"
    ></NavigationButtons>
  </div>
</template>

<style scoped>
.job-list {
  padding-bottom: 230px;
  width: 50%;
  display: flex;
  flex-direction: column;
  gap: 15px;
}
</style>
