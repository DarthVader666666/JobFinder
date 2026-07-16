<script setup>
import "@/assets/main.css";
import SearchBar from "./components/SearchBar.vue";
import SettingsComponent from "./components/SettingsComponent.vue";
import Button from "primevue/button";
import Toast from "primevue/toast";
import PendingModal from "./components/Modals/PendingModal.vue";
import SearchBarModal from "./components/Modals/SearchBarModal.vue";
import { computed, ref } from "vue";
import { useStore } from "vuex";
import { useToast } from "primevue/usetoast";
import FilterModal from "./components/Modals/FilterModal.vue";

const store = useStore();
const toast = useToast();

const finders = computed(() => store.getters.getFinders);
const isPending = computed(() => store.getters.getPending);
const jobs = computed(() => store.getters.getJobs);
const isJobsEmpty = computed(() => jobs.value.length === 0);

const showSearchBar = ref(false);
const showFilter = ref(false);

function showSuccess(summary, detail) {
  toast.add({
    severity: "success",
    summary: summary,
    detail: detail,
    life: 2000,
  });
}

function showError(summary, detail) {
  toast.add({
    severity: "error",
    summary: summary,
    detail: detail,
    life: 2000,
  });
}
</script>

<template>
  <Toast style="width: 320px" />
  <div class="header">
    <div class="title">
      <span style="font-size: 2rem">Find Your Job</span>
      <span>Поиск работы в РБ</span>
    </div>
  </div>
  <div class="main">
    <div class="settings" :class="{ mobileVisible: isJobsEmpty }">
      <SearchBar @showError="showError" @showSuccess="showSuccess"></SearchBar>
      <div class="filter">
        <span>Источники</span>
        <hr />
        <SettingsComponent></SettingsComponent>
      </div>
    </div>

    <div class="job-list" :class="{ mobileVisible: isJobsEmpty }">
      <div v-for="(job, index) in jobs" :key="index">
        <div className="job-item">
          <div class="job-left">
            <a className="job-link" :href="job.link" target="_blank">
              <div class="job-title">
                <span v-if="job.title.includes('Error:')" style="color: red"
                  ><i class="pi pi-exclamation-circle"></i
                  >{{ ` ${job.title}` }}</span
                >
                <span v-else>{{ job.title }}</span>
              </div>
              <div class="job-details">
                <span v-if="job.experience"
                  ><i class="pi pi-briefcase"></i>{{ job.experience }}</span
                >
                <span v-if="job.company"
                  ><i class="pi pi-building"></i>{{ job.company }}</span
                >
                <span v-if="job.location"
                  ><i class="pi pi-map-marker"></i>{{ job.location }}</span
                >
                <span v-if="job.timePosted"
                  ><i class="pi pi-clock"></i>{{ job.timePosted }}</span
                >
              </div>
            </a>
          </div>
          <div class="job-right">
            <span class="salary">{{
              job.salary &&
              `${job.salary.min === job.salary.max ? job.salary.min : job.salary.min + " - " + job.salary.max} ${job.salary.currency}`
            }}</span>
            <a class="job-logo" :href="job.logo?.url ?? ''" target="_blank">
              <img
                v-if="job.logo?.source"
                v-bind:src="
                  finders.find((x) => x.source === job.logo?.source).img
                "
              />
            </a>
          </div>
        </div>
      </div>
    </div>
  </div>
  <div class="buttons" :class="{ mobileVisible: isJobsEmpty }">
    <Button rounded
      ><i class="pi pi-search" @click="() => (showSearchBar = true)"></i
    ></Button>
    <Button rounded
      ><i class="pi pi-sliders-h" @click="() => (showFilter = true)"></i
    ></Button>
  </div>
  <SearchBarModal v-model:visible="showSearchBar"> </SearchBarModal>

  <FilterModal v-model:visible="showFilter"></FilterModal>
  <PendingModal v-model:visible="isPending"></PendingModal>
</template>

<style scoped>
.header {
  display: flex;
}

.title {
  display: flex;
  flex-direction: column;
  gap: 5px;
  padding-left: 20px;
  padding-bottom: 10px;
  color: rgb(16, 185, 129);
  text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.7);
}

.main {
  padding: 20px;
  display: flex;
  flex-direction: row;
  gap: 20px;
}

.settings {
  position: sticky;
  top: 10px;
  align-self: flex-start;
  min-width: 180px;
  width: 25%;
}

.job-list {
  width: 70%;
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.job-item {
  display: flex;
  flex-direction: row;
  width: 100%;
  height: 150px;
  padding: 10px;
  background-color: rgb(215, 215, 215);
  justify-content: space-between;
  border-radius: 10px;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.4);
}

.job-left {
  display: flex;
  flex-direction: column;
  max-width: 70%;
}

.job-details {
  font-size: small;
  align-items: top;
  color: rgb(110, 110, 110);

  span {
    display: inline-block;
    padding-right: 8px;
    max-width: 220px;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    i {
      padding-right: 5px;
      color: rgb(66, 66, 66);
      font-weight: bold;
      font-size: 0.8rem;
      margin: 1px;
    }
  }
}

.job-right {
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  align-items: end;
}

.salary {
  font-size: large;
  font-weight: bold;
  word-break: break-word;
  text-align: end;
}

.job-logo {
  &:hover {
    opacity: 0.7;
  }
  background: white;
  box-shadow: 0 2px 1px rgba(0, 0, 0, 0.4);
  max-width: 80px;
  img {
    width: 100%;
  }
}

.job-link {
  height: 100%;
  width: 100%;
  border-radius: 10px 10px 0 0;
  text-decoration: none;
  color: black;
}

.job-title {
  max-height: 60px;
  padding-bottom: 15px;
  overflow: hidden;
  text-overflow: ellipsis;
}

.job-item:hover {
  background: rgb(233, 233, 233);
}

.job-link:visited {
  color: rgb(116, 116, 116);
}

.buttons {
  display: none;
  position: fixed;
  bottom: 30px;
  right: 30px;
  opacity: 0.7;
  z-index: 1;

  button {
    width: 60px;
    height: 60px;

    i {
      font-size: 1.4rem;
    }
  }
}

.filter {
  padding: 15px;
  text-align: center;
  height: 500px;
  background: white;
  border-radius: 10px;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.4);
}

@media (max-width: 500px) {
  .main {
    padding: 0px;
  }

  .job-list {
    width: 100%;
  }

  .settings {
    display: none;
  }

  .settings.mobileVisible {
    width: 100%;
    display: block; /* show only when finders is empty */
  }

  .buttons {
    display: flex;
    flex-direction: column;
    gap: 15px;
  }

  .buttons.mobileVisible {
    display: none;
  }

  .job-list.mobileVisible {
    display: none;
  }
}
</style>
