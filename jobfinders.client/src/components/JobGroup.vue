<script setup>
import { computed } from "vue";
import { useStore } from "vuex";
import { helper } from "@/helper";

const store = useStore();
const finders = computed(() => store.getters.getFinders);

const props = defineProps({
  jobArray: {
    type: Array,
    default: () => [],
  },
});

function getValue(key) {
  const value = props.jobArray.filter((job) => job[key] !== null);
  return value.length > 0 ? value[0][key] : "";
}

const experience = getValue("experience");
const company = getValue("company");
const location = getValue("location");
const timePosted = getValue("timePosted");
const salary = getValue("salary");
</script>

<template>
  <div className="job-item">
    <div class="job-top">
      <div className="job-link">
        <div class="job-title">
          <span
            v-if="props.jobArray[0].title.includes('Error:')"
            style="color: red"
            ><i class="pi pi-exclamation-circle"></i
            >{{ ` ${props.jobArray[0].title}` }}</span
          >
          <span v-else :title="props.jobArray[0].title">{{
            props.jobArray[0].title
          }}</span>
        </div>
        <div class="job-details">
          <span v-if="experience" :title="experience"
            ><i class="pi pi-briefcase"></i>{{ experience }}</span
          >
          <span v-if="company" :title="company"
            ><i class="pi pi-building"></i>{{ company }}</span
          >
          <span v-if="location" :title="location"
            ><i class="pi pi-map-marker"></i>{{ location }}</span
          >
          <span v-if="timePosted" :title="timePosted"
            ><i class="pi pi-clock"></i>{{ timePosted }}</span
          >
        </div>
      </div>
      <span class="salary">{{ helper.formatSalary(salary) }}</span>
    </div>

    <div class="job-sources">
      <div v-for="(job, index) in props.jobArray" :key="index" class="job-logo">
        <img
          v-if="job.logo?.source"
          v-bind:src="finders.find((x) => x.source === job.logo?.source).img"
        />
      </div>
    </div>
  </div>
</template>

<style scoped>
.job-item {
  display: flex;
  flex-direction: column;
  width: 100%;
  height: 150px;
  padding: 10px;
  background-color: rgb(215, 215, 215);
  justify-content: space-between;
  border-radius: 10px;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.4);
}

.job-top {
  display: flex;
  max-height: 80%;
}

.job-link {
  max-height: 100%;
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
  overflow-wrap: break-word;
  word-wrap: break-word;
  word-break: break-word;
  -webkit-line-break: anywhere;
}

.job-details {
  font-size: small;
  align-items: top;
  color: rgb(110, 110, 110);
  max-height: 55%;
  overflow: hidden;

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

.salary {
  width: 20%;
  font-size: large;
  font-weight: bold;
  word-break: break-word;
  text-align: end;
}

.job-sources {
  display: flex;
  justify-content: end;
}

.job-logo {
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 20px;
  img {
    height: 30px;
    padding: 3px;
  }
}

.job-item:hover {
  background: rgb(233, 233, 233);
  cursor: pointer;
}
</style>
