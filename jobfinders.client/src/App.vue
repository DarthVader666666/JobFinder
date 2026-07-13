<script setup>
import SearchBar from "./components/SearchBar.vue";
import FilterComponent from "./components/FilterComponent.vue";
import Button from "primevue/button";
import Dialog from "primevue/dialog";
import Toast from "primevue/toast";
import { computed, ref } from "vue";
import { useStore } from "vuex";
import { useToast } from "primevue/usetoast";

const toast = useToast();
const store = useStore();

const finders = ref([
  {
    logo: "linkedin-logo-small.png",
    img: "linkedin-logo-large.png",
    source: "LinkedIn",
    active: true,
  },
  {
    logo: "rabotaby-logo-small.png",
    img: "rabotaby-logo-large.png",
    source: "RabotaBy",
    active: true,
  },
  {
    logo: "devby-logo-small.png",
    img: "devby-logo-large.png",
    source: "DevBy",
    active: true,
  },
  {
    logo: "pracaby-logo-small.png",
    img: "pracaby-logo-large.png",
    source: "PracaBy",
    active: true,
  },
  {
    logo: "trabajo-logo-small.png",
    img: "trabajo-logo-large.png",
    source: "Trabajo",
    active: true,
  },
  {
    logo: "bebee-logo-small.png",
    img: "bebee-logo-large.png",
    source: "BeBee",
    active: true,
  },
  {
    logo: "joblum-logo-small.png",
    img: "joblum-logo-large.png",
    source: "Joblum",
    active: true,
  },
]);

const speciality = ref("");
const location = ref(null);
const loading = ref(false);
const showfinders = ref(false);
const jobs = ref([]);
const showSearchBar = ref(false);
const showFilter = ref(false);

const isJobsEmpty = computed(() => jobs.value.length === 0);

async function findJobs() {
  showfinders.value = false;
  showSearchBar.value = false;
  jobs.value = [];

  const bodyValue = {
    speciality: speciality.value.trim(),
    location: location.value?.trim(),
    sources: finders.value.filter((o) => o.active).map((o) => o.source),
  };

  loading.value = true;

  const response = await store.dispatch("getJobs", bodyValue);

  if (response.status === 500) {
    showError("Server error", response.data);
  } else {
    jobs.value = response.data;
    showSuccess(200, "Fetch Successful");
  }

  loading.value = false;
}

function changeSpeciality(value) {
  speciality.value = String(value).trim();
}

function changeLocation(value) {
  location.value = String(value).trim();
}

function checkFinder(finder, checked) {
  var jobFinder = finders.value.find((x) => x === finder);
  jobFinder.active = checked;
}

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
  <Toast />

  <div class="main">
    <div class="settings" :class="{ mobileVisible: isJobsEmpty }">
      <SearchBar
        :changeSpeciality="changeSpeciality"
        :changeLocation="changeLocation"
        :findJobs="findJobs"
      ></SearchBar>
      <FilterComponent :finders="finders" @checkFinder="checkFinder" />
    </div>
    <div class="job-list" :class="{ mobileVisible: isJobsEmpty }">
      <h3 v-if="loading" style="color: red">Loading...</h3>
      <div v-for="(job, index) in jobs" :key="index">
        <div className="job-item">
          <div class="job-left">
            <a className="job-link" :href="job.link" target="_blank">
              <div class="title">
                <span>{{ job.title }}</span>
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
            <a class="job-logo" :href="job.logo.url ?? ''" target="_blank">
              <img
                v-bind:src="
                  finders.find((x) => x.source === job.logo.source).img
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
  <Dialog
    v-if="showSearchBar"
    style="width: 90%"
    v-model:visible="showSearchBar"
    modal
    @hide="
      () => {
        showSearchBar = false;
      }
    "
    :draggable="false"
  >
    <template #header>
      <span style="width: 90%"></span>
    </template>
    <SearchBar
      :changeSpeciality="changeSpeciality"
      :changeLocation="changeLocation"
      :findJobs="findJobs"
    ></SearchBar>
  </Dialog>

  <Dialog
    v-if="showFilter"
    style="width: 90%"
    v-model:visible="showFilter"
    modal
    @hide="
      () => {
        showFilter = false;
      }
    "
    :draggable="false"
  >
    <template #header>
      <span style="width: 90%"></span>
    </template>
    <FilterComponent :finders="finders" @checkFinder="checkFinder" />
  </Dialog>
</template>

<style scoped>
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

.title {
  padding-bottom: 15px;
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
