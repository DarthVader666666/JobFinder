<script setup>
import Dialog from "primevue/dialog";
import JobList from "../JobList.vue";

const props = defineProps({
  jobGroup: {
    type: Array,
    default: () => [],
  },
  toast: {
    type: Object,
    default: null,
  },
});
</script>
<template>
  <Dialog
    modal
    :draggable="false"
    style="
      background: transparent;
      border: 0;
      box-shadow: none;
      min-width: 50%;
      width: 490px;
    "
  >
    <template #header>
      <span style="width: 90%"></span>
    </template>
    <JobList
      :jobs="props.jobGroup"
      :usePagination="false"
      :toast="props.toast"
    ></JobList>
  </Dialog>
</template>

<style scoped></style>
