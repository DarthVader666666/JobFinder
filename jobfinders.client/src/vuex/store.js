import { createStore } from "vuex";
import axios from "axios";

const store = createStore({
  state: {
    serverUrl: import.meta.env.VITE_API_URL,
    nbrbCurrRateUrl: "https://api.nbrb.by/exrates/rates?periodicity=0",
    pending: false,
    sending: false,
    showSearchBarModal: false,
    showSettingsModal: false,
    finders: [
      {
        img: "rabotaby-logo-large.png",
        source: "RabotaBy",
        active: true,
      },
      {
        img: "pracaby-logo-large.png",
        source: "PracaBy",
        active: true,
      },

      {
        img: "bebee-logo-large.png",
        source: "BeBee",
        active: true,
      },
      {
        img: "joblum-logo-large.png",
        source: "Joblum",
        active: true,
      },
      {
        img: "belmeta-logo-large.png",
        source: "Belmeta",
        active: true,
      },
      {
        img: "rework-logo-large.png",
        source: "Rework",
        active: true,
      },
      {
        img: "gsz-logo-large.png",
        source: "GSZ",
        active: true,
      },
    ],
    jobsRequest: {
      speciality: "",
      location: "",
      sources: [],
      filter: {
        exactTitle: false,
        salaryDefined: false,
        orderBySalary: false,
        salary: {
          min: null,
          max: null,
          currency: null,
          rates: [],
        },
      },
    },
    bufferedJobs: [],
    filteredJobs: [],
    savedJobs: [],
    allFindersChecked: true,
    currencies: ["$", "BYN", "€", "₽", "Нет"],
    apiCurrencies: ["USD", "EUR", "RUB"],
    oldCurrency: "Нет",
    selectedCurrency: "Нет",
    currencyData: {
      date: null,
      rates: null,
    },
    range: [0.2, 100],
    rangeMultiplier: 100,
    savedJobsShown: false,
  },
  getters: {
    getPending(state) {
      return state.pending;
    },
    getSending(state) {
      return state.sending;
    },
    getSpeciality(state) {
      return state.jobsRequest.speciality;
    },
    getLocation(state) {
      return state.jobsRequest.location;
    },
    getExactTitle(state) {
      return state.jobsRequest.filter.exactTitle;
    },
    getSalaryDefined(state) {
      return state.jobsRequest.filter.salaryDefined;
    },
    getOrderBySalary(state) {
      return state.jobsRequest.filter.orderBySalary;
    },
    getFinders(state) {
      return state.finders;
    },
    getBufferedJobs(state) {
      return state.bufferedJobs;
    },
    getFilteredJobs(state) {
      return state.filteredJobs;
    },
    getSavedJobs(state) {
      state.savedJobs = JSON.parse(localStorage.getItem("savedJobs") || "[]");
      return state.savedJobs;
    },
    getAllFindersChecked(state) {
      return state.finders.every((x) => x.active);
    },
    getShowSearchBarModal(state) {
      return state.showSearchBarModal;
    },
    getShowSettingsModal(state) {
      return state.showSettingsModal;
    },
    getJobsRequest(state, getters) {
      return {
        speciality: state.jobsRequest.speciality.trim(),
        location: state.jobsRequest.location.trim(),
        sources: state.finders.filter((f) => f.active).map((f) => f.source),
        filter: {
          exactTitle: state.jobsRequest.filter.exactTitle,
          salaryDefined: state.jobsRequest.filter.salaryDefined,
          orderBySalary: state.jobsRequest.filter.orderBySalary,
          salary: {
            min: state.range[0] * getters.getRangeMultiplier,
            max: state.range[1] * getters.getRangeMultiplier,
            currency: state.selectedCurrency,
          },
          currencyRates:
            getters.getCurrencyData.rates?.map((x) => ({
              Abbreviation: x.Cur_Abbreviation,
              Rate: x.Cur_OfficialRate,
              Scale: x.Cur_Scale,
            })) ?? [],
        },
      };
    },
    getSelectedCurrency(state) {
      return state.selectedCurrency;
    },
    getCurrencyData(state) {
      return state.currencyData;
    },
    getUsdRate(state) {
      return state.currencyData.rates.find((x) => x.Cur_Abbreviation === "USD")
        .Cur_OfficialRate;
    },
    getEurRate(state) {
      return state.currencyData.rates.find((x) => x.Cur_Abbreviation === "EUR")
        .Cur_OfficialRate;
    },
    getRubRate(state) {
      return state.currencyData.rates.find((x) => x.Cur_Abbreviation === "RUB")
        .Cur_OfficialRate;
    },
    getRange(state) {
      return state.range;
    },
    getRangeMultiplier(state) {
      if (state.selectedCurrency === "₽") {
        return state.rangeMultiplier * 100;
      } else if (
        state.selectedCurrency === "Нет" ||
        state.selectedCurrency === "BYN"
      ) {
        return state.rangeMultiplier * 5;
      } else {
        return state.rangeMultiplier;
      }
    },
    getSavedJobsShown(state) {
      return state.savedJobsShown;
    },
  },
  mutations: {
    setPending(state, value) {
      state.pending = value;
    },
    setSending(state, value) {
      state.sending = value;
    },
    setSpeciality(state, value) {
      state.jobsRequest.speciality = value;
    },
    setLocation(state, value) {
      state.jobsRequest.location = value;
    },
    setExactTitle(state, value) {
      state.jobsRequest.filter.exactTitle = value;
    },
    setSalaryDefined(state, value) {
      state.jobsRequest.filter.salaryDefined = value;
    },
    setOrderBySalary(state, value) {
      state.jobsRequest.filter.orderBySalary = value;
    },
    checkFinder(state, payload) {
      const finder = state.finders.find((x) => x.source === payload.source);
      finder.active = payload.active;
    },
    setBufferedJobs(state, value) {
      state.bufferedJobs = [];
      value.forEach((x) => state.bufferedJobs.push(x));

      const links = this.getters.getSavedJobs.map((sj) => sj.link);

      state.bufferedJobs.forEach((bj) => {
        if (links.includes(bj.link)) {
          bj.saved = true;
        } else {
          bj.saved = false;
        }
      });
    },
    setFilteredJobs(state, value) {
      state.filteredJobs = [];
      value.forEach((x) => state.filteredJobs.push(x));

      const links = this.getters.getSavedJobs.map((sj) => sj.link);

      state.filteredJobs.forEach((fj) => {
        if (links.includes(fj.link)) {
          fj.saved = true;
        } else {
          fj.saved = false;
        }
      });
    },
    setSavedJobs(state, value) {
      state.savedJobs = value;
      localStorage.setItem("savedJobs", JSON.stringify(value));
    },
    setAllFindersChecked(state, value) {
      state.allFindersChecked = value;

      state.finders.forEach((x) => {
        x.active = value;
      });
    },
    setShowSearchBarModal(state, value) {
      state.showSearchBarModal = value;
    },
    setShowSettingsModal(state, value) {
      state.showSettingsModal = value;
    },
    setSelectedCurrency(state, value) {
      state.selectedCurrency = value;
    },
    setCurrencyData(state, value) {
      const now = new Date();
      const currentDate = new Date(
        now.getFullYear(),
        now.getMonth(),
        now.getDate(),
      );
      state.currencyData.date = currentDate;
      state.currencyData.rates = value.filter((x) =>
        state.apiCurrencies.includes(x.Cur_Abbreviation),
      );
      state.currencyData.rates.push({
        Cur_Abbreviation: "BYN",
        Cur_OfficialRate: 1,
        Cur_Scale: 1,
      });
    },
    setRange(state, value) {
      state.range = value;
    },
    setSavedJobsShown(state, value) {
      state.savedJobsShown = value;
    },
  },
  actions: {
    async getFetch({ commit }, { url, usePending, func }) {
      if (usePending) {
        commit("setPending", true);
      }
      return await axios
        .get(url, {
          headers: { "Content-Type": "application/json" },
        })
        .then(async (response) => {
          if (response.status === 200) {
            func(response.data);
          }
        })
        .catch((error) => {
          if (error.response) {
            return { status: error.response.status };
          }
        })
        .finally(() => {
          if (usePending) {
            commit("setPending", false);
          }
        });
    },
    async downloadJobs({ state, commit, getters }) {
      commit("setPending", true);
      return await axios
        .post(`${state.serverUrl}/jobs/getjobs`, getters.getJobsRequest, {
          headers: { "Content-Type": "application/json" },
        })
        .then(async (response) => {
          if (response.status === 200) {
            commit("setFilteredJobs", response.data);
            commit("setBufferedJobs", response.data);
            return { status: response.status };
          }
        })
        .catch((error) => {
          if (error.response) {
            commit("setFilteredJobs", [error.response.data.errorText]);
            return { status: error.response.status };
          }
        })
        .finally(() => {
          commit("setPending", false);
        });
    },
    showSuccess(_, { toast, summary, detail }) {
      toast.add({
        severity: "success",
        summary: summary,
        detail: detail,
        life: 2000,
      });
    },
    showError(_, { toast, summary, detail }) {
      toast.add({
        severity: "error",
        summary: summary,
        detail: detail,
        life: 2000,
      });
    },
    async downloadCurrencyRates({ state, commit, dispatch }, toast) {
      await axios
        .get(`${state.nbrbCurrRateUrl}`, {
          headers: { "Content-Type": "application/json" },
        })
        .then(async (response) => {
          if (response.status === 200) {
            commit("setCurrencyData", response.data);
          }
        })
        .catch((error) => {
          if (error.response) {
            dispatch("showError", {
              toast: toast,
              summary: "Error",
              detail: "Не обновились курсы валют",
            });
          }
        });
    },
    updateFilteredJobs({ state, commit }) {
      var jobs = [];
      state.bufferedJobs
        .filter((j) =>
          state.finders
            .filter((f) => f.active)
            .map((f) => f.source)
            .includes(j.source),
        )
        .forEach((j) => jobs.push(j));

      const keys = Object.keys(state.jobsRequest.filter) ?? [];

      keys.forEach((key) => {
        if (state.jobsRequest.filter[key]) {
          if (key === "exactTitle") {
            jobs = jobs.filter((fj) =>
              fj.title
                .toLowerCase()
                .includes(state.jobsRequest.speciality.toLowerCase()),
            );
          }

          if (key === "orderBySalary") {
            jobs = jobs.sort(
              (x, y) => (y.salary?.max ?? 0) - (x.salary?.max ?? 0),
            );
          }

          if (key === "salaryDefined") {
            jobs = jobs.filter(
              (fj) =>
                fj.salary?.currency &&
                fj.salary.min >=
                  state.range[0] * this.getters.getRangeMultiplier &&
                fj.salary.max <=
                  state.range[1] * this.getters.getRangeMultiplier,
            );
          }

          commit("setFilteredJobs", jobs);
        }
      });
    },
    addSavedJob({ state, getters, commit }, job) {
      const savedJobs = getters.getSavedJobs;
      savedJobs.push(job);
      commit("setSavedJobs", savedJobs);
      commit("setFilteredJobs", state.filteredJobs);
      commit("setBufferedJobs", state.bufferedJobs);
    },
    removeSavedJob({ state, commit, getters }, job) {
      const savedJobs = getters.getSavedJobs.filter((x) => x.link !== job.link);
      commit("setSavedJobs", savedJobs);
      commit("setFilteredJobs", state.filteredJobs);
      commit("setBufferedJobs", state.bufferedJobs);
    },
    showSavedJobs({ state, commit }, value) {
      value === undefined
        ? commit("setSavedJobsShown", !state.savedJobsShown)
        : commit("setSavedJobsShown", value);

      if (state.savedJobsShown) {
        window.scrollTo({ top: 0, behavior: "smooth" });
      }
    },
    async sendComment({ state, commit, dispatch }, { request, toast }) {
      commit("setSending", true);
      await axios
        .post(`${state.serverUrl}/user/sendComment`, request, {
          headers: { "Content-Type": "application/json" },
        })
        .then(async (response) => {
          if (response.status === 200) {
            dispatch("showSuccess", {
              toast: toast,
              summary: "Спасибо!",
              detail: "Сообщение отправлено",
            });
          }
        })
        .catch((error) => {
          if (error.response) {
            dispatch("showError", {
              toast: toast,
              summary: "Ошибка",
              detail: "Не удалось отправить сообщение",
            });
          }
        })
        .finally(() => {
          commit("setSending", false);
        });
    },
  },
});

export default store;
