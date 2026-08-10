import { createStore } from "vuex";
import axios from "axios";
import { helper } from "@/helper";

const store = createStore({
  state: {
    serverUrl: import.meta.env.VITE_API_URL,
    nbrbCurrRateUrl: "https://api.nbrb.by/exrates/rates?periodicity=",
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
      {
        img: "headhunter-logo-large.png",
        source: "Headhunter",
        active: true,
      },
    ],
    filter: {
      exactTitle: false,
      orderBySalary: false,
      groupBySource: false,
    },
    request: {
      speciality: "",
      location: "",
    },
    bufferedJobs: [],
    filteredJobs: [],
    savedJobs: [],
    allFindersChecked: true,
    currencies: ["BYN", "$", "€", "₽"],
    apiCurrencies: ["USD", "EUR", "RUB", "KZT", "GEL", "AZN", "UZS"],
    selectedCurrency: "BYN",
    currencyData: {
      date: null,
      rates: null,
    },
    range: [0, 100],
    rangeMultiplier: 50,
    infinity: 100000000,
    savedJobsShown: false,
    abortController: null,
  },
  getters: {
    getPending(state) {
      return state.pending;
    },
    getSending(state) {
      return state.sending;
    },
    getSpeciality(state) {
      return state.request.speciality;
    },
    getLocation(state) {
      return state.request.location;
    },
    getExactTitle(state) {
      return state.filter.exactTitle;
    },
    getOrderBySalary(state) {
      return state.filter.orderBySalary;
    },
    getGroupBySource(state) {
      return state.filter.groupBySource;
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
      return state.savedJobs;
    },
    getSavedJobsCache(state) {
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
        sources: state.finders.filter((f) => f.active).map((f) => f.source),
        speciality: state.request.speciality.trim(),
        location: state.request.location.trim(),
        salary: {
          min: state.range[0] * getters.getRangeMultiplier,
          max:
            state.range[1] === 100
              ? state.infinity
              : state.range[1] * getters.getRangeMultiplier,
          currency: state.selectedCurrency,
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
      } else if (state.selectedCurrency === "BYN") {
        return state.rangeMultiplier * 3;
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
      state.request.speciality = value;
    },
    setLocation(state, value) {
      state.request.location = value;
    },
    setExactTitle(state, value) {
      state.filter.exactTitle = value;
    },
    setOrderBySalary(state, value) {
      state.filter.orderBySalary = value;
    },
    setGroupBySource(state, value) {
      state.filter.groupBySource = value;
    },
    checkFinder(state, payload) {
      const finder = state.finders.find((x) => x.source === payload.source);
      finder.active = payload.active;
    },
    setBufferedJobs(state, value) {
      state.bufferedJobs = [];
      value.forEach((x) => state.bufferedJobs.push(x));
    },
    setFilteredJobs(state, value) {
      state.filteredJobs = [];
      value.forEach((x) => state.filteredJobs.push(x));
    },
    setSavedJobsCache(state, value) {
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
      state.currencyData.rates = value;
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
    setAbortController(state, value) {
      state.abortController = value;
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
    async downloadJobs({ state, commit, dispatch, getters }) {
      if (state.abortController) {
        state.abortController.abort();
      }

      const abortController = new AbortController();
      commit("setAbortController", abortController);
      commit("setPending", true);

      return await axios
        .post(`${state.serverUrl}/jobs/getjobs`, getters.getJobsRequest, {
          signal: abortController.signal,
          headers: { "Content-Type": "application/json" },
        })
        .then((response) => {
          if (response.status === 200) {
            commit("setFilteredJobs", response.data);
            commit("setBufferedJobs", response.data);
            helper.convertSalaries(state.selectedCurrency);
            helper.checkSavedJobs();
            dispatch("updateFilteredJobs");
            store.dispatch("showSavedJobs", false);
            return { status: response.status };
          }
        })
        .catch((error) => {
          if (error.response) {
            commit("setFilteredJobs", [
              error.response.data.errorText ?? error.response.data,
            ]);
            return {
              status: error.response.status,
              error: error.response.data.errorText ?? error.response.data,
            };
          }

          if (axios.isCancel(error)) {
            return { status: 499 };
          }
        })
        .finally(() => {
          commit("setAbortController", null);
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
    showInfo(_, { toast, summary, detail }) {
      toast.add({
        severity: "info",
        summary: summary,
        detail: detail,
        life: 2000,
      });
    },
    showWarning(_, { toast, summary, detail }) {
      toast.add({
        severity: "warn",
        summary: summary,
        detail: detail,
        life: 2000,
      });
    },
    async downloadCurrencyRates({ state, commit, dispatch }, toast) {
      async function getRates(periodicity) {
        try {
          const response = await axios.get(
            `${state.nbrbCurrRateUrl}${periodicity}`,
            {
              headers: { "Content-Type": "application/json" },
            },
          );

          if (response.status === 200) {
            return response.data.filter((x) =>
              state.apiCurrencies.includes(x.Cur_Abbreviation),
            );
          }

          return [];
        } catch (error) {
          if (error.response) {
            dispatch("showError", {
              toast: toast,
              summary: "Error",
              detail: "Не обновились курсы валют",
            });
          }

          return [];
        }
      }

      var rates = [...(await getRates(0)), ...(await getRates(1))];
      commit("setCurrencyData", rates);
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

      const keys = Object.keys(state.filter) ?? [];

      keys.forEach((key) => {
        if (!state.filter[key]) {
          return;
        }
        if (key === "exactTitle") {
          jobs = jobs.filter((job) => {
            const specialityParts = state.request.speciality
              .split(/[ -]/)
              .map((x) => x.toLowerCase());
            const titleParts = job.title
              .split(/[ -]/)
              .map((x) => x.toLowerCase());

            return specialityParts.some((sp) =>
              titleParts.some((tp) => tp.includes(sp)),
            );
          });
        }

        if (key === "orderBySalary") {
          jobs = jobs.sort(
            (x, y) => (y.salary?.max ?? 0) - (x.salary?.max ?? 0),
          );
        }

        if (key === "groupBySource") {
          jobs = jobs.sort((x, y) => x.source.localeCompare(y.source));
        }

        if (state.range[0]) {
          const minRange = state.range[0] * this.getters.getRangeMultiplier;
          const maxRange =
            state.range[1] < 100
              ? state.range[1] * this.getters.getRangeMultiplier
              : state.infinity;

          jobs = jobs.filter(
            (fj) =>
              fj.salary?.currency &&
              (fj.salary?.min === fj.salary?.max
                ? fj.salary?.min >= minRange
                : true) &&
              fj.salary?.min <= maxRange &&
              fj.salary?.max >= minRange &&
              (fj.salary?.min <= maxRange ? true : fj.salary?.max <= maxRange),
          );
        } else {
          jobs = jobs.filter(
            (fj) =>
              !fj.salary ||
              fj.salary?.max <=
                (state.range[1] === 100
                  ? state.infinity
                  : state.range[1] * this.getters.getRangeMultiplier),
          );
        }

        commit("setFilteredJobs", jobs);
      });
    },
    addSavedJob({ state, commit }, job) {
      state.savedJobs.push(job);
      helper.checkUncheckSavedJob(job);
      commit("setSavedJobsCache", state.savedJobs);
    },
    removeSavedJob({ state, commit }, job) {
      state.savedJobs.splice(state.savedJobs.indexOf(job), 1);
      helper.checkUncheckSavedJob(job);
      commit("setSavedJobsCache", state.savedJobs);
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
