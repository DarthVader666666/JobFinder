<script setup>
import { useToast } from "primevue/usetoast";
import Checkbox from "primevue/checkbox";
import Select from "primevue/select";
import Slider from "primevue/slider";
import { computed } from "vue";
import { useStore } from "vuex";
import { helper } from "@/helper";

const props = defineProps({
  disableFilter: {
    type: Boolean,
    default: false,
  },
});

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

const range = computed({
  get: () => store.getters.getRange,
  set: (value) => store.commit("setRange", value),
});

const rangeMultiplier = computed(() => store.getters.getRangeMultiplier);

const selectedCurrency = computed(() => store.getters.getSelectedCurrency);

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
  updateFilteredJobs(orderBySalary.value, false);
}

function updateFilteredJobs(value, scrollUp = true) {
  if (store.getters.getBufferedJobs?.length) {
    if (!value) {
      store.commit("setFilteredJobs", store.getters.getBufferedJobs);
    }

    store.dispatch("updateFilteredJobs");

    if (scrollUp) {
      window.scrollTo({ top: 0, behavior: "smooth" });
    }
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
        :disabled="props.disableFilter"
      ></Checkbox>
    </div>

    <div>
      <span>сначала высокая з/п</span>
      <Checkbox
        v-model="orderBySalary"
        @change="updateFilteredJobs(orderBySalary)"
        binary
        :disabled="props.disableFilter"
      ></Checkbox>
    </div>
    <div class="currency">
      <span>Конверсия</span>
      <Select
        v-model="currency"
        :options="store.state.currencies"
        @update:modelValue="setCurrencyValues($event)"
        :disabled="props.disableFilter"
      />
    </div>
  </div>
  <hr />
  <div class="range">
    <div style="display: flex; gap: 10px">
      <span>уровень з/п</span>
      <Checkbox
        v-model="salaryDefined"
        @change="
          updateFilteredJobs(
            salaryDefined,
            store.getters.getShowSettingsModal && salaryDefined,
          )
        "
        binary
        :disabled="props.disableFilter"
      ></Checkbox>
    </div>
    <div class="min-max">
      <span :style="{ opacity: !salaryDefined ? 0.4 : 1 }"
        >{{ range[0] * rangeMultiplier
        }}<span>{{
          selectedCurrency === "Нет" ? "" : " " + selectedCurrency
        }}</span></span
      >
      <span
        v-if="range[1] < 100"
        :style="{ opacity: !salaryDefined ? 0.4 : 1, paddingBottom: '5px' }"
        >{{ range[1] * rangeMultiplier
        }}<span>{{
          selectedCurrency === "Нет" ? "" : " " + selectedCurrency
        }}</span>
      </span>
      <i
        v-else
        class="icon-infinity"
        :style="{ opacity: !salaryDefined ? 0.4 : 1, fontSize: '1.5rem' }"
      ></i>
    </div>

    <Slider
      v-model="range"
      range
      :disabled="!salaryDefined || props.disableFilter"
      @slideend="() => updateFilteredJobs(salaryDefined)"
    ></Slider>
  </div>
</template>

<style scoped>
.filter {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(110px, 1fr));
  gap: 15px;
  padding: 5px;
  font-size: 0.9rem;

  div {
    display: flex;
    align-items: center;
    justify-content: space-between;

    max-width: 120px;
    gap: 5px;
  }
}

.currency {
  display: flex;
  flex-direction: column;

  .p-select {
    width: 90px;
  }

  .p-select :deep(span) {
    padding: 5px 0 5px 5px;
    font-size: 0.9rem;
  }
}

.range {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 5px;
  font-size: 0.9rem;

  .p-slider {
    width: 90%;
    height: 6px;
  }

  .min-max {
    display: flex;
    justify-content: space-between;
    font-size: 0.9rem;
    width: 90%;
  }

  .icon-infinity {
    width: 1.3rem;
    height: 1.3rem;
    background-size: contain;
    background-image: url("/infinity.png");
  }
}
</style>
