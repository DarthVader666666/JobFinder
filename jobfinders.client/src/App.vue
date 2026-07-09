<script setup>
import { ref } from "vue";
import CriteriaInput from "./components/CriteriaInput.vue";

const url = import.meta.env.VITE_API_URL;

const jobFinders = ref([
  { logo: "linkedin-logo-small.png", img: "linkedin-logo-large.png", source: "LinkedIn", active: false },
  { logo: "rabotaby-logo-small.png", img: "rabotaby-logo-large.png", source: "RabotaBy", active: false },
  { logo: "devby-logo-small.png",    img: "devby-logo-large.png",    source: "DevBy",    active: false },
  { logo: "pracaby-logo-small.png",  img: "pracaby-logo-large.png",  source: "PracaBy",  active: false },
  { logo: "trabajo-logo-small.png",  img: "trabajo-logo-large.png",  source: "Trabajo",  active: false },
  { logo: "bebee-logo-small.png",    img: "bebee-logo-large.png",    source: "BeBee",    active: false },
  { logo: "joblum-logo-small.png",   img: "joblum-logo-large.png",   source: "Joblum",   active: false },
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
    sources: jobFinders.value
      .filter((o) => o.active)
      .map((o) => o.source),
  };

  loading.value = true;

  const response = await fetch(`${url}/Jobs/GetJobs`, {
    method: "POST",
    body: JSON.stringify(bodyValue),
    headers: { "Content-Type": "application/json" },
  }).then((r) => r.json());

  jobFinders.value.forEach((jf) => {
    if (jf.active) {
      const jobsGroup = response.find((job) => job.source === jf.source);
      if (jobsGroup) {
        jobs.value.push({
          logo: jf.logo,
          source: jf.source,
          link: jobsGroup.link,
          jobs: jobsGroup.jobs,
        });
      }
    }
  });

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
    <button @click="() => showJobFinders = !showJobFinders" className="menu-button">Show Job Finders</button>
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
    <div style="display: flex; flex-direction: column">
      <div
        v-for="(response, index) in jobs"
        :key="index"
        style="display: flex; flex-direction: column"
      >
        <div className="list-box">
          <a :href="response.link" target="_blank">
            <img v-bind:src="response.logo" width="50px" height="50px" />
          </a>
        </div>
        <div>
          <a
            v-for="(job, index) in response.jobs"
            :key="index"
            className="job-link"
            :href="job.link"
            target="_blank"
          >
            <span style="font-weight: bold">{{ job.title }}</span>
            <span>{{ job.salary && `${(job.salary.min === job.salary.max ? job.salary.min : job.salary.min + ' - ' + job.salary.max)} ${job.salary.currency}` }}</span>
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

.list-box {
  flex-direction: row;
  padding-top: 8px;
  padding-bottom: 8px;
  align-content: start;
  gap: 2px;
}

.job-link {
  min-height: 18px;
  display: block;
  text-decoration: none;
  color: black;
  padding: 3px;
  margin: 2px;
  background-color: gray;
}

.job-link:hover {
  color: lightgray;
  background: #333;
}
.job-link:visited {
  color: darkgray;
  font-style: italic;
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
</style>
