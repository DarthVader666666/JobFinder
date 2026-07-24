import store from "./vuex/store";

export const helper = {
  convertSalaries(selectedCurrency) {
    var apiCurrency = this.getNbrbApiCurrency(selectedCurrency);

    if (apiCurrency === "Нет") {
      store.state.filteredJobs.forEach((job) => {
        if (job.salary) {
          job.salary.max = job.originalSalary.max;
          job.salary.min = job.originalSalary.min;
          job.salary.currency = job.originalSalary.currency;
        }
      });
    } else {
      store.state.filteredJobs.forEach((job) => {
        if (
          job.salary &&
          job.salary.currency &&
          job.salary.currency != selectedCurrency
        ) {
          const jobCurrency = this.getNbrbApiCurrency(job.salary.currency);
          this.convert(job, jobCurrency, selectedCurrency, apiCurrency);
        }
      });
    }
  },
  convert(job, jobCurrency, selectedCurrency, apiCurrency) {
    if (job.originalSalary?.currency === selectedCurrency) {
      job.salary.min = job.originalSalary.min;
      job.salary.max = job.originalSalary.max;
      job.salary.currency = job.originalSalary.currency;

      return;
    }

    const bynData = { Cur_OfficialRate: 1, Cur_Scale: 1 };

    const jobCurrencyData =
      jobCurrency === "BYN"
        ? bynData
        : store.getters.getCurrencyData.rates.find(
            (x) => x.Cur_Abbreviation === jobCurrency,
          );

    const apiCurrencyData =
      apiCurrency === "BYN"
        ? bynData
        : store.getters.getCurrencyData.rates.find(
            (x) => x.Cur_Abbreviation === apiCurrency,
          );

    const jobRate =
      jobCurrencyData.Cur_OfficialRate / jobCurrencyData.Cur_Scale;
    const convertRate =
      apiCurrencyData.Cur_OfficialRate / apiCurrencyData.Cur_Scale;

    const rate = jobRate / convertRate;

    job.salary.min = Math.round(job.salary.min * rate);
    job.salary.max = Math.round(job.salary.max * rate);
    job.salary.currency = selectedCurrency;
  },
  getNbrbApiCurrency(currency) {
    var apiCurrency = "Нет";

    switch (currency) {
      case "$":
        apiCurrency = "USD";
        break;
      case "€":
        apiCurrency = "EUR";
        break;
      case "₽":
        apiCurrency = "RUB";
        break;
      case "BYN":
        apiCurrency = "BYN";
        break;
      default:
        apiCurrency = "Нет";
    }

    return apiCurrency;
  },
  async updateCurrencyRates() {
    const now = new Date();
    const currentDate = new Date(
      now.getFullYear(),
      now.getMonth(),
      now.getDate(),
    );

    const currencyData = store.getters.getCurrencyData;

    if (
      currencyData.rates === null ||
      currencyData.date === null ||
      currencyData.date < currentDate
    ) {
      await store.dispatch("downloadCurrencyRates");
    }
  },
};
