<script setup>
import { ref } from "vue";
import CriteriaInput from "./components/CriteriaInput.vue";

const url = import.meta.env.VITE_API_URL;

const sources = [
  { name: "linkedIn", img: "linkedin-logo-large.png" },
  { name: "rabotaBy", img: "rabotaby-logo-large.png" },
  { name: "devBy", img: "devby-logo-large.png" },
  { name: "pracaBy", img: "pracaby-logo-large.png" },
  { name: "trabajo", img: "trabajo-logo-large.png" },
  { name: "beBee", img: "bebee-logo-large.png" },
  { name: "joblum", img: "joblum-logo-large.png" },
];

const searchSourceOptions = ref([
  { img: "linkedin-logo-small.png", name: "linkedIn", active: false },
  { img: "rabotaby-logo-small.png", name: "rabotaBy", active: false },
  { img: "devby-logo-small.png", name: "devBy", active: false },
  { img: "pracaby-logo-small.png", name: "pracaBy", active: false },
  { img: "trabajo-logo-small.png", name: "trabajo", active: false },
  { img: "bebee-logo-small.png", name: "beBee", active: false },
  { img: "joblum-logo-small.png", name: "joblum", active: false },
]);

const speciality = ref("");
const area = ref("");
const loading = ref(false);
const show = ref(false);
const responses = ref([]);

async function findJobs() {
  show.value = false;
  responses.value = [];

  const bodyValue = {
    speciality: speciality.value.trim(),
    area: area.value.trim(),
    sources: searchSourceOptions.value
      .filter((o) => o.active)
      .map((o) => o.name),
  };

  loading.value = true;

  const loadedResponses = await fetch(`${url}/Jobs/GetList`, {
    method: "POST",
    body: JSON.stringify(bodyValue),
    headers: { "Content-Type": "application/json" },
  }).then((r) => r.json());

  searchSourceOptions.value.forEach((option) => {
    if (option.active) {
      const match = loadedResponses.find((lr) => lr.sourceName === option.name);
      if (match) {
        responses.value.push({
          img: option.img,
          sourceName: option.name,
          sourceUrl: match.sourceUrl,
          jobs: match.jobs,
        });
      }
    }
  });

  loading.value = false;
}

function changeSpeciality(value) {
  speciality.value = String(value).trim();
}

function changeArea(value) {
  area.value = String(value).trim();
}

function setSearchSource(value) {
  const option = searchSourceOptions.value.find((o) => o.name === value);
  if (option) option.active = !option.active;
}

function isSearchSourceOptionActive(value) {
  return searchSourceOptions.value.find((o) => o.name === value)?.active;
}

function showResources() {
  show.value = !show.value;
}
</script>

<template>
  <div className="head">
    <h3>Welcome to Job Finder!</h3>
    <button @click="showResources" className="menu-button">Resources</button>
  </div>

  <div className="sources" v-show="show">
    <section
      v-for="(source, index) in sources"
      @click="setSearchSource(source.name)"
      :key="index"
      :className="isSearchSourceOptionActive(source.name) ? 'active' : ''"
    >
      <img v-bind:src="source.img" :alt="source.name" />
    </section>
  </div>

  <CriteriaInput
    :changeSpeciality="changeSpeciality"
    :changeArea="changeArea"
    :findJobs="findJobs"
  ></CriteriaInput>

  <div>
    <h3 v-if="loading">Loading...</h3>
    <div style="display: flex; flex-direction: column">
      <div
        v-for="(response, index) in responses"
        :key="index"
        style="display: flex; flex-direction: column"
      >
        <div className="list-box">
          <a :href="response.sourceUrl" target="_blank">
            <img v-bind:src="response.img" width="50px" height="50px" />
          </a>
        </div>
        <div>
          <a
            v-for="(job, index) in response.jobs"
            :key="index"
            className="jobLink"
            :href="job.link"
            target="_blank"
          >
            <span style="font-weight: bold">{{ job.title }}</span>
            <span>{{ job.salary }}</span>
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

.jobLink {
  max-height: 18px;
  display: block;
  text-decoration: none;
  color: black;
  padding: 3px;
  margin: 2px;
  background-color: gray;
}

.jobLink:hover {
  color: lightgray;
  background: #333;
}
.jobLink:visited {
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

.sources {
  display: flex;
  flex-direction: column;
  gap: 0.2rem;
  width: fit-content;
  height: fit-content;
  padding: 10px;
  float: inline-end;
}

.sources section {
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

.sources section img {
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
