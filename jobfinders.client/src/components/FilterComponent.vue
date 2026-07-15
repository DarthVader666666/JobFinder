<script setup>
import Checkbox from "primevue/checkbox";
import ToggleSwitch from "primevue/toggleswitch";

const props = defineProps({
  finders: {
    typeof: Array,
    default: [],
  },
  allFindersChecked: {
    typeof: Boolean,
    default: false,
  },
});

const emit = defineEmits(["checkFinder", "setAllFinders"]);
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
      :modelValue="props.allFindersChecked"
      @update:modelValue="emit('setAllFinders', $event)"
    />
    <span>Все</span>
  </div>
  <div class="finders">
    <div
      class="finder-option"
      v-for="(finder, index) in props.finders"
      :key="index"
    >
      <img v-bind:src="finder.img" :alt="finder.source" />
      <Checkbox
        @update:modelValue="emit('checkFinder', finder, $event)"
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
