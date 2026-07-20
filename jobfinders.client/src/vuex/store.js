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
      exactTitle: false,
      salaryDefined: false,
    },
    orderBySalary: false,
    jobs: [],
    allFindersChecked: true,
    currencies: ["$", "BYN", "€", "₽", "Все"],
    oldCurrency: "Все",
    selectedCurrency: "Все",
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
      return state.jobsRequest.exactTitle;
    },
    getSalaryDefined(state) {
      return state.jobsRequest.salaryDefined;
    },
    getOrderBySalary(state) {
      return state.orderBySalary;
    },
    getFinders(state) {
      return state.finders;
    },
    getJobs(state) {
      return state.orderBySalary
        ? state.jobs.sort((x, y) => (y.salary?.max ?? 0) - (x.salary?.max ?? 0))
        : state.jobs;
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
        exactTitle: state.jobsRequest.exactTitle,
        salaryDefined: state.jobsRequest.salaryDefined,
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
      state.jobsRequest.exactTitle = value;
    },
    setSalaryDefined(state, value) {
      state.jobsRequest.salaryDefined = value;
    },
    setOrderBySalary(state, value) {
      state.orderBySalary = value;
    },
    checkFinder(state, payload) {
      const finder = state.finders.find((x) => x.source === payload.source);
      finder.active = payload.active;
    },
    setJobs(state, value) {
      state.jobs = value;
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
            commit("setJobs", response.data);
            return { status: response.status };
          }
        })
        .catch((error) => {
          if (error.response) {
            commit("setJobs", [error.response.data.errorText]);
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
  },
});

export default store;
