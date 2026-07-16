<script setup>
import Checkbox from "primevue/checkbox";
import { computed } from "vue";
import { useStore } from "vuex";

const store = useStore();

const exactTitle = computed({
  get: () => store.getters.getExactTitle,
  set: (value) => store.commit("setExactTitle", value),
});

const salaryDefined = computed({
  get: () => store.getters.getSalaryDefined,
  set: (value) => store.commit("setSalaryDefined", value),
});

const orderBySalary = computed({
  get: () => store.getters.getOrderBySalary,
  set: (value) => store.commit("setOrderBySalary", value),
});
</script>

<template>
  <div class="filter">
    <div>
      <span>точное совпадение</span>
      <Checkbox v-model="exactTitle" binary></Checkbox>
    </div>
    <div>
      <span>з/п указана</span>
      <Checkbox v-model="salaryDefined" binary></Checkbox>
    </div>
    <div>
      <span>сначала высокая з/п</span>
      <Checkbox v-model="orderBySalary" binary></Checkbox>
    </div>
  </div>
</template>

<style scoped>
.filter {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(110px, 1fr));
  gap: 25px;

  div {
    display: flex;
    flex-wrap: nowrap;
    align-items: center;
    gap: 5px;
  }
}
</style>
