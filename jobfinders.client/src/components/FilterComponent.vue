<script setup>
import Checkbox from "primevue/checkbox";

const props = defineProps({
  finders: {
    typeof: Array,
    default: [],
  },
});

const emit = defineEmits(["checkFinder"]);
</script>

<template>
  <div class="filter">
    <span>Источники</span>
    <hr />
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
  </div>
</template>

<style scoped>
.filter {
  padding: 15px;
  text-align: center;
  height: 500px;
  background: white;
  border-radius: 10px;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.4);
}

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
