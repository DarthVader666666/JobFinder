<script setup>
import Checkbox from "primevue/checkbox";
import ToggleSwitch from "primevue/toggleswitch";
import { computed } from "vue";
import { useStore } from "vuex";

const props = defineProps({
  disableSources: {
    type: Boolean,
    default: false,
  },
});

const store = useStore();
const finders = computed(() => store.getters.getFinders);
const allFindersChecked = computed(() => store.getters.getAllFindersChecked);

function checkFinder(finder, checked) {
  store.commit("checkFinder", { source: finder.source, active: checked });
  store.dispatch("updateFilteredJobs");
  window.scrollTo({ top: 0, behavior: "smooth" });
}

function toggleAllSources(value) {
  store.commit("setAllFindersChecked", value);
  store.dispatch("updateFilteredJobs");
  window.scrollTo({ top: 0, behavior: "smooth" });
}
</script>

<template>
  <div class="sources">
    <div class="sources-toggle">
      <ToggleSwitch
        :modelValue="allFindersChecked"
        @update:modelValue="toggleAllSources($event)"
        :disabled="props.disableSources"
      />
      <span>Все</span>
    </div>
  </div>
  <div class="finder-options">
    <div class="finder-option" v-for="(finder, index) in finders" :key="index">
      <img v-bind:src="finder.img" :alt="finder.source" />
      <Checkbox
        @update:modelValue="checkFinder(finder, $event)"
        :binary="true"
        :modelValue="finder.active"
        :disabled="props.disableSources"
      />
    </div>
  </div>
</template>

<style scoped>
.sources {
  display: flex;
  gap: 5px;
  align-items: center;
  justify-content: center;
}

.sources-toggle {
  display: flex;
  align-items: center;
  gap: 10px;
  padding-bottom: 10px;
}

.finder-options {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(110px, 1fr));
  gap: 15px;
  padding: 10px;

  img {
    width: 70px;
    height: 20px;
  }
}

.finder-option {
  display: flex;
  gap: 15px;
}
</style>
