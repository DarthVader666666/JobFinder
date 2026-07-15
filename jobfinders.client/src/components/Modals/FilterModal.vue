<script setup>
import Dialog from "primevue/dialog";
import FilterComponent from "../FilterComponent.vue";

const props = defineProps({
  finders: {
    typeof: Array,
    default: [],
  },
  allFindersChecked: {
    typeof: Boolean,
  },
  checkFinder: {
    typeof: Function,
    default: null,
  },
  setAllFinders: {
    typeof: Function,
    default: null,
  },
});
</script>

<template>
  <Dialog style="width: 90%" :draggable="false" modal>
    <template #header>
      <span style="width: 90%"></span>
    </template>
    <FilterComponent
      :finders="props.finders"
      :allFindersChecked="allFindersChecked"
      @setAllFinders="setAllFinders"
      @checkFinder="checkFinder"
    />
  </Dialog>
</template>

<style></style>
