import { createStore } from "vuex";
import axios from "axios";

const store = createStore({
  state: {
    serverUrl: import.meta.env.VITE_API_URL,
  },
  getters: {},
  mutations: {},
  actions: {
    async getJobs({ state }, request) {
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
        });
    },
  },
});

export default store;
