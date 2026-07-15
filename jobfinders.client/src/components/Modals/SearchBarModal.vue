<script setup>
import SearchBar from "../SearchBar.vue";
import Dialog from "primevue/dialog";

const props = defineProps({
  changeSpeciality: {
    type: Function,
    required: true,
  },
  changeLocation: {
    type: Function,
    required: true,
  },
  findJobs: {
    type: Function,
    required: true,
  },
});
</script>
<template>
  <Dialog
    style="width: 90%"
    v-model:visible="showSearchBar"
    modal
    :draggable="false"
  >
    <template #header>
      <span style="width: 90%"></span>
    </template>
    <SearchBar
      @changeSpeciality="props.changeSpeciality"
      @changeLocation="props.changeLocation"
      @findJobs="props.findJobs"
    ></SearchBar>
  </Dialog>
</template>
<style scoped></style>
