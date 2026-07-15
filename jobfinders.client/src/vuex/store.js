import { createStore } from "vuex";
import axios from "axios";

const store = createStore({
  state: {
    serverUrl: import.meta.env.VITE_API_URL,
    pending: false,
  },
  getters: {
    getPending(state) {
      return state.pending;
    },
  },
  mutations: {
    setPending(state, value) {
      state.pending = value;
    },
  },
  actions: {
    async getJobs({ state, commit }, request) {
      commit("setPending", true);
      return await axios
        .post(`${state.serverUrl}/jobs/getjobs`, request, {
          headers: { "Content-Type": "application/json" },
        })
        .then(async (response) => {
          if (response.status === 200) {
            return { status: response.status, data: response.data };
          }
        })
        .catch((error) => {
          if (error.response) {
            return {
              status: error.response.status,
              data: error.response.data.errorText,
            };
          }
        })
        .finally(() => {
          commit("setPending", false);
        });
    },
  },
});

export default store;
