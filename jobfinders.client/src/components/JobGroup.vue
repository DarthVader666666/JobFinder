<script setup>
import { computed, ref } from "vue";
import { useStore } from "vuex";
import { helper } from "@/helper";
import JobGroupModal from "./Modals/JobGroupModal.vue";

const store = useStore();

const props = defineProps({
  jobGroup: {
    type: Array,
    default: () => [],
  },
  toast: {
    type: Object,
    default: null,
  },
});

const showJobGroupModal = ref(false);

const logos = computed(() => updateLogos());
const finders = computed(() => store.getters.getFinders);

function updateLogos() {
  if (!props.jobGroup.length) {
    return [];
  }

  var logos = [];
  props.jobGroup
    .map((job) => job.logo)
    .forEach((logo) => {
      if (!logos.map((l) => l.source).includes(logo.source)) {
        logos.push(logo);
      }
    });

  return logos;
}

function getValue(key) {
  return props.jobGroup[0][key] ?? "";
}

const title = computed(() => getValue("title"));
const experience = computed(() => getValue("experience"));
const company = computed(() => getValue("company"));
const location = computed(() => getValue("location"));
const timePosted = computed(() => getValue("timePosted"));
const salary = computed(() => getValue("salary"));

function showJobGroupModalHandler() {
  showJobGroupModal.value = true;
}
</script>

<template>
  <div
    class="job-group"
    :class="showJobGroupModal ? 'invisible' : ''"
    @click="showJobGroupModalHandler"
  >
    <div class="group-back"></div>
    <div class="group-middle"></div>
    <div class="group-front">
      <div class="group-top">
        <div>
          <div class="group-title">
            <span v-if="title.includes('Error:')" style="color: red"
              ><i class="pi pi-exclamation-circle"></i>{{ ` ${title}` }}</span
            >
            <span v-else :title="title">{{ title }}</span>
          </div>
          <div class="group-details">
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
        <div class="salary">
          <span>{{ helper.formatSalary(salary) }}</span>
        </div>
      </div>
      <div class="group-bottom">
        <div v-for="(logo, index) in logos" :key="index" class="job-logo">
          <img
            v-if="logo.source"
            v-bind:src="finders.find((x) => x.source === logo.source).img"
          />
        </div>
      </div>
    </div>
  </div>
  <JobGroupModal
    v-model:visible="showJobGroupModal"
    :jobGroup="props.jobGroup"
    :toast="props.toast"
  >
  </JobGroupModal>
</template>

<style scoped>
.job-group {
  position: relative;
  width: 100%;
  height: 150px;
}

.group-back {
  position: absolute;
  top: 0;
  right: 0;
  height: 140px;
  width: 98%;
  background-color: rgb(200, 200, 200);
  border-radius: 10px;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.4);
}

.group-middle {
  position: absolute;
  top: 3%;
  right: 1%;
  height: 140px;
  width: 98%;
  background-color: rgb(200, 200, 200);
  border-radius: 10px;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.4);
}

.group-front {
  position: absolute;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  bottom: 0;
  left: 0;
  height: 140px;
  width: 98%;
  padding: 10px;
  background-color: rgb(215, 215, 215);
  border-radius: 10px;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.4);

  &:hover {
    background: rgb(233, 233, 233);
    cursor: pointer;
  }
}

.group-top {
  display: flex;
  justify-content: space-between;
  max-height: 80%;
}

.group-title {
  max-height: 60px;
  padding-bottom: 15px;
  overflow: hidden;
  text-overflow: ellipsis;
  overflow-wrap: break-word;
  word-wrap: break-word;
  word-break: break-word;
  -webkit-line-break: anywhere;
}

.group-details {
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
  width: 30%;
  font-size: large;
  font-weight: bold;
  word-break: break-word;
  text-align: end;
}

.group-bottom {
  display: flex;
  gap: 3px;
  justify-content: end;
  height: 20%;
  overflow: hidden;
}

.job-logo {
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 20px;

  img {
    max-height: 20px;
    width: auto;
    padding: 3px;
  }
}

.invisible :deep(*) {
  background: transparent;
  color: transparent;

  &:hover {
    background: transparent;
  }

  img,
  i {
    display: none;
  }
}
</style>
