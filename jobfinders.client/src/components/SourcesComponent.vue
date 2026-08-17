<script setup>
import { helper } from "@/helper";
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

const emit = defineEmits(["resetFirstPage"]);

const store = useStore();
const finders = computed(() => store.getters.getFinders);
const allFindersChecked = computed(() => store.getters.getAllFindersChecked);

function checkFinder(finder, checked) {
  store.commit("checkFinder", { source: finder.source, active: checked });
  store.dispatch("updateFilteredJobs");
  emit("resetFirstPage");
  store.commit("setHasMoreJobs", false);
  helper.scrollUp();
}

function toggleAllSources(value) {
  store.commit("setAllFindersChecked", value);
  store.dispatch("updateFilteredJobs");
  emit("resetFirstPage");
  store.commit("setHasMoreJobs", false);
  helper.scrollUp();
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
      <span>Все источники</span>
    </div>
  </div>
  <div class="finder-options">
    <div class="finder-option" v-for="(finder, index) in finders" :key="index">
      <div class="logo">
        <label :for="finder.source">
          <img v-bind:src="finder.img" :alt="finder.source" />
        </label>
      </div>

      <Checkbox
        @update:modelValue="checkFinder(finder, $event)"
        :binary="true"
        :modelValue="finder.active"
        :disabled="props.disableSources"
        :inputId="finder.source"
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
  gap: 14px;
  padding: 10px;
}

.logo {
  align-content: center;
  text-align: end;
  width: 60px;

  img {
    max-height: 20px;
    max-width: 60px;
  }
}

.finder-option {
  display: flex;
  gap: 15px;
}
</style>
