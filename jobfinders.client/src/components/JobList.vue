<script setup>
import { computed, watch } from "vue";
import JobItem from "./JobItem.vue";
import NavigationButtons from "./NavigationButtons.vue";
import { useStore } from "vuex";
import { helper } from "@/helper.js";
import JobGroup from "./JobGroup.vue";

const store = useStore();

const props = defineProps({
  toast: {
    type: Object,
    default: null,
  },
  savedJobsShown: {
    type: Boolean,
    default: false,
  },
  jobs: {
    type: Array,
    default: () => [],
  },
  usePagination: {
    type: Boolean,
    default: true,
  },
});

const firstPage = computed({
  get: () => store.getters.getFirstPage,
  set: (value) => store.commit("setFirstPage", value),
});

const rows = computed({
  get: () => store.getters.getRows,
  set: (value) => store.commit("setRows", value),
});

const slicedJobs = computed(() =>
  props.usePagination
    ? props.jobs.slice(
        firstPage.value * rows.value,
        firstPage.value * rows.value + rows.value,
      )
    : props.jobs,
);

const isFirstPage = computed(() => firstPage.value <= 0);
const isLastPage = computed(
  () =>
    firstPage.value * rows.value + slicedJobs.value.length >= props.jobs.length,
);

const jobsRange = computed(() => [
  firstPage.value * rows.value + 1,
  firstPage.value * rows.value + slicedJobs.value.length,
]);

watch(rows, (newValue) => {
  if (newValue > props.jobs.length) {
    firstPage.value = 0;
  }
});

function saveJob(job) {
  var savedJob;

  if (slicedJobs.value[0]?.length ?? false) {
    slicedJobs.value.forEach((jobGroup) => {
      var j = jobGroup.find((sj) => helper.areJobsEqual(sj, job));

      if (j) {
        savedJob = j;
        return;
      }
    });
  } else {
    savedJob = slicedJobs.value.find((sj) => helper.areJobsEqual(sj, job));
  }

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
    firstPage.value * rows.value + slicedJobs.value.length < props.jobs.length
  ) {
    firstPage.value++;
  }

  helper.scrollUp();
}
</script>
<template>
  <div class="list">
    <div v-for="(job, index) in slicedJobs" :key="index">
      <JobItem v-if="savedJobsShown" :job="job" @saveJob="saveJob"></JobItem>
      <div v-else>
        <JobGroup
          v-if="job.length && job.length > 1"
          :jobGroup="job"
          :toast="props.toast"
        ></JobGroup>
        <JobItem
          v-else
          :job="job.length ? job[0] : job"
          @saveJob="saveJob"
        ></JobItem>
      </div>
    </div>
    <NavigationButtons
      v-if="props.usePagination"
      :isFirstPage="isFirstPage"
      :isLastPage="isLastPage"
      :jobsRange="jobsRange"
      :showNavigationButtons="slicedJobs.length > 0"
      @navigationHandler="navigationHandler"
    ></NavigationButtons>
  </div>
</template>

<style scoped>
.list {
  display: flex;
  flex-direction: column;
  gap: 15px;
}
</style>
