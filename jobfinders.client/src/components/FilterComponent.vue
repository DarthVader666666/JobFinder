<script setup>
import { useToast } from "primevue/usetoast";
import Checkbox from "primevue/checkbox";
import Select from "primevue/select";
import Slider from "primevue/slider";
import { computed, ref } from "vue";
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

const selectedCurrency = computed(() => store.getters.getSelectedCurrency);

const range = ref([1, 100]);

async function setCurrencyValues(selectedSalary) {
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

  helper.convertSalaries(selectedSalary);
  store.commit("setOrderBySalary", false);
  store.commit("setShowSettingsModal", false);
}

function updateFilteredJobs(value) {
  if (store.getters.getBufferedJobs?.length) {
    if (!value) {
      store.commit("setFilteredJobs", store.getters.getBufferedJobs);
    }

    store.dispatch("updateFilteredJobs");
    store.commit("setShowSettingsModal", false);

    window.scrollTo({ top: 0, behavior: "smooth" });
  }
}
</script>

<template>
  <div class="filter">
    <div>
      <span>точное совпадение</span>
      <Checkbox
        v-model="exactTitle"
        @change="updateFilteredJobs(exactTitle)"
        binary
      ></Checkbox>
    </div>

    <div>
      <span>сначала высокая з/п</span>
      <Checkbox
        v-model="orderBySalary"
        @change="updateFilteredJobs(orderBySalary)"
        binary
      ></Checkbox>
    </div>
    <div class="currency">
      <span>Конвертировать</span>
      <Select
        v-model="currency"
        :options="store.state.currencies"
        @update:modelValue="setCurrencyValues($event)"
      />
    </div>
    <div>
      <span>з/п указана</span>
      <Checkbox
        v-model="salaryDefined"
        @change="updateFilteredJobs(salaryDefined)"
        binary
      ></Checkbox>
    </div>
  </div>
  <hr />
  <div class="range">
    <div class="min-max">
      <span>{{ range[0] * 100 }}</span>
      <span>{{ selectedCurrency === "Нет" ? "" : selectedCurrency }}</span>
      <span>{{ range[1] * 100 }}</span>
    </div>

    <Slider v-model="range" range :disabled="!salaryDefined"></Slider>
  </div>
</template>

<style scoped>
.filter {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(110px, 1fr));
  gap: 25px;
  padding: 5px 0 5px 0;

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

.range {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 15px;
  padding: 10px;

  .p-slider {
    width: 90%;
    height: 6px;
  }

  .min-max {
    display: flex;
    justify-content: space-between;
    width: 90%;
  }
}
</style>
