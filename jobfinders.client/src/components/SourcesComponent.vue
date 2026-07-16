<script setup>
import Checkbox from "primevue/checkbox";
import ToggleSwitch from "primevue/toggleswitch";
import { computed } from "vue";
import { useStore } from "vuex";

const store = useStore();
const finders = computed(() => store.getters.getFinders);
const allFindersChecked = computed(() => store.getters.getAllFindersChecked);

function checkFinder(finder, checked) {
  store.commit("checkFinder", { source: finder.source, active: checked });
}

function setAllFinders(toggled) {
  finders.value.forEach((finder) => {
    store.commit("checkFinder", { source: finder.source, active: toggled });
  });
}
</script>

<template>
  <div
    style="
      display: flex;
      gap: 5px;
      align-items: center;
      justify-content: center;
    "
  >
    <ToggleSwitch
      :modelValue="allFindersChecked"
      @update:modelValue="setAllFinders($event)"
    />
    <span>Все</span>
  </div>
  <div class="finders">
    <div class="finder-option" v-for="(finder, index) in finders" :key="index">
      <img v-bind:src="finder.img" :alt="finder.source" />
      <Checkbox
        @update:modelValue="checkFinder(finder, $event)"
        :binary="true"
        :modelValue="finder.active"
      />
    </div>
  </div>
</template>

<style scoped>
.finders {
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
