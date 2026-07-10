<script setup>
import { ref } from "vue";
import CriteriaInput from "./components/CriteriaInput.vue";

const url = import.meta.env.VITE_API_URL;

const jobFinders = ref([
  {
    logo: "linkedin-logo-small.png",
    img: "linkedin-logo-large.png",
    source: "LinkedIn",
    active: false,
  },
  {
    logo: "rabotaby-logo-small.png",
    img: "rabotaby-logo-large.png",
    source: "RabotaBy",
    active: false,
  },
  {
    logo: "devby-logo-small.png",
    img: "devby-logo-large.png",
    source: "DevBy",
    active: false,
  },
  {
    logo: "pracaby-logo-small.png",
    img: "pracaby-logo-large.png",
    source: "PracaBy",
    active: false,
  },
  {
    logo: "trabajo-logo-small.png",
    img: "trabajo-logo-large.png",
    source: "Trabajo",
    active: false,
  },
  {
    logo: "bebee-logo-small.png",
    img: "bebee-logo-large.png",
    source: "BeBee",
    active: false,
  },
  {
    logo: "joblum-logo-small.png",
    img: "joblum-logo-large.png",
    source: "Joblum",
    active: false,
  },
]);

const speciality = ref("");
const location = ref(null);
const loading = ref(false);
const showJobFinders = ref(false);
const jobs = ref([]);

async function findJobs() {
  showJobFinders.value = false;
  jobs.value = [];

  const bodyValue = {
    speciality: speciality.value.trim(),
    location: location.value?.trim(),
    sources: jobFinders.value.filter((o) => o.active).map((o) => o.source),
  };

  loading.value = true;

  jobs.value = await fetch(`${url}/Jobs/GetJobs`, {
    method: "POST",
    body: JSON.stringify(bodyValue),
    headers: { "Content-Type": "application/json" },
  }).then((r) => r.json());

  loading.value = false;
}

function changeSpeciality(value) {
  speciality.value = String(value).trim();
}

function changeLocation(value) {
  location.value = String(value).trim();
}

function activateJobFinder(value) {
  const jobFinder = jobFinders.value.find((jf) => jf.source === value);
  if (jobFinder) jobFinder.active = !jobFinder.active;
}

function isJobFinderActive(value) {
  return jobFinders.value.find((jf) => jf.source === value)?.active;
}
</script>

<template>
  <div className="head">
    <h3>Welcome to Job Finder!</h3>
    <button
      @click="() => (showJobFinders = !showJobFinders)"
      className="menu-button"
    >
      Show Job Finders
    </button>
  </div>

  <div className="job-finders" v-show="showJobFinders">
    <section
      v-for="(jobFinder, index) in jobFinders"
      @click="activateJobFinder(jobFinder.source)"
      :key="index"
      :className="isJobFinderActive(jobFinder.source) ? 'active' : ''"
    >
      <img v-bind:src="jobFinder.img" :alt="jobFinder.source" />
    </section>
  </div>

  <CriteriaInput
    :changeSpeciality="changeSpeciality"
    :changeLocation="changeLocation"
    :findJobs="findJobs"
  ></CriteriaInput>

  <div>
    <h3 v-if="loading">Loading...</h3>
    <div class="job-list">
      <div v-for="(job, index) in jobs" :key="index">
        <div className="job-item">
          <a className="job-link" :href="job.link" target="_blank">
            <div class="title-salary">
              <span class="title">{{ job.title }}</span>
              <span class="salary">{{
                job.salary &&
                `${job.salary.min === job.salary.max ? job.salary.min : job.salary.min + " - " + job.salary.max} ${job.salary.currency}`
              }}</span>
            </div>
          </a>
          <a :href="job.logo.url ?? ''" target="_blank" class="job-item-logo">
            <img
              v-bind:src="
                jobFinders.find((x) => x.source === job.logo.source).img
              "
            />
          </a>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
div {
  display: flex;
  flex-wrap: wrap;
}

.head {
  flex-direction: row;
  align-items: center;
  justify-content: space-between;
}

.job-list {
  width: 70%;
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.job-item {
  width: 100%;
  max-height: 100px;
  flex-direction: column;
  align-content: start;
  background-color: rgb(185, 185, 185);
  justify-content: space-between;
  border-radius: 10px;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.4);
}

.job-link {
  height: 70px;
  width: 100%;
  border-radius: 10px 10px 0 0;
  display: block;
  text-decoration: none;
  color: black;
}

.title-salary {
  display: flex;
  flex-direction: row;
  justify-content: space-between;
}

.title {
  padding: 10px;
}

.salary {
  font-size: large;
  font-weight: bold;
  max-width: fit-content;
  padding: 10px;
}

.job-item:hover {
  background: rgb(156, 156, 156);
}
.job-link:visited {
  color: rgb(116, 116, 116);
  font-style: italic;
}

.job-item-logo {
  width: 50px;
  height: 20px;
  min-width: 50px;
  max-width: 70px;
  position: sticky;
  z-index: 1;
}

.job-item-logo img {
  &:hover {
    opacity: 0.7;
  }
  background: white;
  width: 100%;
  height: 100%;
  box-shadow: 0 2px 1px rgba(0, 0, 0, 0.4);
}

h1,
h3 {
  color: rgb(180, 29, 29);
  font-weight: bold;
  border-radius: 10px;
  width: fit-content;
  padding: 5px;
  text-align: center;
  text-shadow: 0.1rem 0.1rem black;
}

.job-finders {
  display: flex;
  flex-direction: column;
  gap: 0.2rem;
  width: fit-content;
  height: fit-content;
  padding: 10px;
  float: inline-end;
}

.job-finders section {
  width: 10rem;
  height: 3rem;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  border: 3px solid black;
  margin: 0.1rem;
  background-color: rgb(0, 164, 164);
  border-radius: 10px;
}

.job-finders section img {
  height: 95%;
  width: 95%;
}

section.active {
  background-color: aqua;
  cursor: pointer;
}

section:hover {
  cursor: pointer;
}

.menu-button {
  height: 50px;
  background-color: rgb(16, 16, 181);
  color: rgb(224, 223, 242);
  border-radius: 15%;
  font-weight: bold;
  font-size: 1rem;
}

.menu-button:hover {
  cursor: pointer;
}

.menu-button:active {
  background-color: rgba(16, 16, 181, 0.658);
}

@media (max-width: 600px) {
  .job-list {
    width: 100%;
  }
}
</style>
