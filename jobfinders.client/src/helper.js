import store from "./vuex/store";

export const helper = {
  convertSalaries(selectedCurrency) {
    var convertCurrency = this.getNbrbApiCurrency(selectedCurrency);

    if (convertCurrency === "Нет") {
      store.state.jobs.forEach((job) => {
        if (job.salary) {
          job.salary.max = job.originalSalary.max;
          job.salary.min = job.originalSalary.min;
          job.salary.currency = job.originalSalary.currency;
        }
      });
    } else {
      store.state.jobs.forEach((job) => {
        if (
          job.salary &&
          job.salary.currency &&
          job.salary.currency != selectedCurrency
        ) {
          const jobCurrency = this.getNbrbApiCurrency(job.salary.currency);
          this.convert(job, jobCurrency, selectedCurrency, convertCurrency);
        }
      });
    }
  },
  convert(job, jobCurrency, selectedCurrency, convertCurrency) {
    const bynData = { Cur_OfficialRate: 1, Cur_Scale: 1 };

    const jobCurrencyData =
      jobCurrency === "BYN"
        ? bynData
        : store.getters.getCurrencyData.rates.find(
            (x) => x.Cur_Abbreviation === jobCurrency,
          );

    const convertCurrencyData =
      convertCurrency === "BYN"
        ? bynData
        : store.getters.getCurrencyData.rates.find(
            (x) => x.Cur_Abbreviation === convertCurrency,
          );

    const jobRate =
      jobCurrencyData.Cur_OfficialRate / jobCurrencyData.Cur_Scale;
    const convertRate =
      convertCurrencyData.Cur_OfficialRate / convertCurrencyData.Cur_Scale;

    const rate = jobRate / convertRate;

    job.salary.min = Math.round(job.salary.min * rate);
    job.salary.max = Math.round(job.salary.max * rate);
    job.salary.currency = selectedCurrency;
  },
  getNbrbApiCurrency(currency) {
    var convertCurrency = "Нет";

    switch (currency) {
      case "$":
        convertCurrency = "USD";
        break;
      case "€":
        convertCurrency = "EUR";
        break;
      case "₽":
        convertCurrency = "RUB";
        break;
      case "BYN":
        convertCurrency = "BYN";
        break;
      default:
        convertCurrency = "Нет";
    }

    return convertCurrency;
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
