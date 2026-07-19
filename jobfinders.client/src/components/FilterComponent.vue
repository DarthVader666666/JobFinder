<script setup>
import { useToast } from "primevue/usetoast";
import Checkbox from "primevue/checkbox";
import Select from "primevue/select";
import { computed } from "vue";
import { useStore } from "vuex";
import { helper } from "@/helper";

const store = useStore();
const toast = useToast();

const currency = computed({
  get: () => store.getters.getSelectedCurrency,
  set: (value) => store.commit("setSelectedCurrency", value),
});

const currencyData = computed({
  get: () => store.getters.getCurrencyData,
  set: (value) => store.commit("setCurrencyData", value),
});

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

async function setCurrencyValues() {
  const now = new Date();
  const currentDate = new Date(
    now.getFullYear(),
    now.getMonth(),
    now.getDate(),
  );

  if (
    currencyData.value.rates === null ||
    currencyData.value.date === null ||
    currencyData.value.date < currentDate
  ) {
    await store.dispatch("downloadCurrencyRates", toast);
  }

  helper.convertSalaries();
}
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
    <div class="currency">
      <span>Валюта</span>
      <Select
        v-model="currency"
        :options="store.state.currencies"
        @update:modelValue="setCurrencyValues"
      />
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

.currency {
  display: flex;
  flex-direction: column;
  .p-select {
    width: 110px;
  }
}
</style>
