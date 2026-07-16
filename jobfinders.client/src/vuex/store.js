import { createStore } from "vuex";
import axios from "axios";

const store = createStore({
  state: {
    serverUrl: import.meta.env.VITE_API_URL,
    pending: false,
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
    ],
    jobsRequest: {
      speciality: "",
      location: "",
      sources: [],
      exactTitle: false,
      salaryDefined: false,
    },
    jobs: [],
    allFindersChecked: true,
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
    getFinders(state) {
      return state.finders;
    },
    getJobs(state) {
      return state.jobs;
    },
    getAllFindersChecked(state) {
      return state.finders.every((x) => x.active);
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
  },
  actions: {
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
  },
});

export default store;
