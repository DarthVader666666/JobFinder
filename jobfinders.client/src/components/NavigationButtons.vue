<script setup>
import Select from "primevue/select";
import Button from "primevue/button";
import { computed } from "vue";
import { useStore } from "vuex";

const props = defineProps({
  isFirstPage: {
    type: Boolean,
    default: false,
  },
  isLastPage: {
    type: Boolean,
    default: false,
  },
  jobsRange: {
    type: Array,
    default: () => [],
  },
  showNavigationButtons: {
    type: Boolean,
    default: false,
  },
});

const store = useStore();

const rows = computed({
  get: () => store.getters.getRows,
  set: (value) => store.commit("setRows", value),
});

const emit = defineEmits(["navigationHandler", "changeRows"]);
</script>

<template>
  <div v-if="showNavigationButtons" class="navigation-buttons">
    <Select
      style="position: sticky; width: 90px; top: 0"
      v-model="rows"
      :options="[10, 20, 30, 40, 50]"
    ></Select>
    <div style="display: flex; gap: 15px; align-items: center">
      <Button
        rounded
        icon="pi pi-arrow-left"
        :disabled="props.isFirstPage"
        @click="emit('navigationHandler', 'back')"
      ></Button>
      <span>{{ props.jobsRange[0] }} - {{ props.jobsRange[1] }} </span>
      <Button
        rounded
        icon="pi pi-arrow-right"
        :disabled="props.isLastPage"
        @click="emit('navigationHandler', 'forward')"
      ></Button>
    </div>
  </div>
</template>

<style scoped>
.navigation-buttons {
  position: sticky;
  bottom: 15px;
  display: flex;
  flex-direction: column;
  gap: 10px;
  align-items: center;
  opacity: 0.7;

  span {
    background: rgb(16 185 129);
    color: white;
    padding: 5px 10px 5px 10px;
    border-radius: 15px;
  }
}
</style>
