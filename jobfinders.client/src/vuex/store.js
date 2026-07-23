import { createStore } from "vuex";
import axios from "axios";

const store = createStore({
  state: {
    serverUrl: import.meta.env.VITE_API_URL,
    nbrbCurrRateUrl: "https://api.nbrb.by/exrates/rates?periodicity=0",
    pending: false,
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
    ],
    jobsRequest: {
      speciality: "",
      location: "",
      sources: [],
      filter: {
        exactTitle: false,
        salaryDefined: false,
        orderBySalary: false,
      },
    },
    bufferedJobs: [],
    filteredJobs: [],
    allFindersChecked: true,
    currencies: ["$", "BYN", "€", "₽", "Нет"],
    oldCurrency: "Нет",
    selectedCurrency: "Нет",
    currencyData: {
      date: null,
      rates: null,
    },
  },
  getters: {
    getPending(state) {
      return state.pending;
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
    getAllFindersChecked(state) {
      return state.finders.every((x) => x.active);
    },
    getShowSearchBarModal(state) {
      return state.showSearchBarModal;
    },
    getShowSettingsModal(state) {
      return state.showSettingsModal;
    },
    getJobsRequest(state) {
      return {
        speciality: state.jobsRequest.speciality.trim(),
        location: state.jobsRequest.location.trim(),
        sources: state.finders.filter((f) => f.active).map((f) => f.source),
        filter: {
          exactTitle: state.jobsRequest.filter.exactTitle,
          salaryDefined: state.jobsRequest.filter.salaryDefined,
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
  },
  mutations: {
    setPending(state, value) {
      state.pending = value;
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
      state.bufferedJobs = value;
    },
    setFilteredJobs(state, value) {
      state.filteredJobs = value;
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
      state.currencyData.rates = value;
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
    async downloadJobs({ state, commit }, request) {
      commit("setPending", true);
      return await axios
        .post(`${state.serverUrl}/jobs/getjobs`, request, {
          headers: { "Content-Type": "application/json" },
        })
        .then(async (response) => {
          if (response.status === 200) {
            commit("setBufferedJobs", response.data);
            commit("setFilteredJobs", response.data);
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
          commit("setSelectedCurrency", "Нет");
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
    updateFilteredJobs({ state, commit }, filter) {
      var jobs = [];
      const key = Object.keys(filter)[0];

      if (key === "exactTitle" && filter[key]) {
        jobs = state.filteredJobs.filter((fj) =>
          fj.title.includes(state.jobsRequest.speciality),
        );
      }

      if (key === "salaryDefined" && filter[key]) {
        jobs = state.filteredJobs.filter((fj) => fj.salary?.currency);
      }

      if (key === "orderBySalary" && filter[key]) {
        jobs = state.filteredJobs.sort(
          (x, y) => (y.salary?.max ?? 0) - (x.salary?.max ?? 0),
        );
      }

      if (filter[key]) {
        commit("setBufferedJobs", state.filteredJobs);
        commit("setFilteredJobs", jobs);
      } else {
        commit("setFilteredJobs", state.bufferedJobs);
      }
    },
  },
});

export default store;
