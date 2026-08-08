<script setup>
import { computed } from "vue";
import { useStore } from "vuex";
import Button from "primevue/button";

const store = useStore();
const finders = computed(() => store.getters.getFinders);

const props = defineProps({
  job: {
    type: Object,
  },
});

const emit = defineEmits(["saveJob"]);

function getSalary(salary) {
  if (salary) {
    return `${
      props.job.salary.min === props.job.salary.max
        ? getLongSalary(props.job.salary.min)
        : getLongSalary(props.job.salary.min) +
          " - " +
          getLongSalary(props.job.salary.max)
    } ${props.job.salary.currency}`;
  }
}

function getLongSalary(value) {
  if (value >= 10000) {
    return `${Math.round(value / 1000)}K`;
  }
  return value;
}
</script>

<template>
  <div className="job-item">
    <div class="job-left">
      <a className="job-link" :href="props.job.link" target="_blank">
        <div class="job-title">
          <span v-if="props.job.title.includes('Error:')" style="color: red"
            ><i class="pi pi-exclamation-circle"></i
            >{{ ` ${props.job.title}` }}</span
          >
          <span v-else :title="props.job.title">{{ props.job.title }}</span>
        </div>
        <div class="job-details">
          <span v-if="props.job.experience" :title="props.job.experience"
            ><i class="pi pi-briefcase"></i>{{ props.job.experience }}</span
          >
          <span v-if="props.job.company" :title="props.job.company"
            ><i class="pi pi-building"></i>{{ props.job.company }}</span
          >
          <span v-if="props.job.location" :title="props.job.location"
            ><i class="pi pi-map-marker"></i>{{ props.job.location }}</span
          >
          <span v-if="props.job.timePosted" :title="props.job.timePosted"
            ><i class="pi pi-clock"></i>{{ props.job.timePosted }}</span
          >
        </div>
      </a>
    </div>
    <div class="job-right">
      <span class="salary">{{ getSalary(props.job.salary) }}</span>
      <div>
        <div style="padding-bottom: 5px; text-align: end">
          <Button
            icon="pi pi-bookmark"
            style="border-color: rgb(100, 116, 139, 0.6)"
            rounded
            :outlined="props.job.saved ? false : true"
            :severity="props.job.saved ? 'info' : 'secondary'"
            title="сохранить"
            @click="emit('saveJob')"
          ></Button>
        </div>
        <a class="job-logo" :href="props.job.logo?.url ?? ''" target="_blank">
          <img
            v-if="props.job.logo?.source"
            v-bind:src="
              finders.find((x) => x.source === props.job.logo?.source).img
            "
          />
        </a>
      </div>
    </div>
  </div>
</template>

<style scoped>
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
  width: 100%;
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
  min-width: 15%;
}

.salary {
  font-size: large;
  font-weight: bold;
  word-break: break-word;
  text-align: end;
}

.job-logo {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 80px;
  border-radius: 20px;
  &:hover {
    background-color: rgb(248, 250, 252);
    /* box-shadow: 0px 1px 6px rgba(0, 0, 0, 0.3); */
  }
  img {
    width: 90%;
    height: 30px;
    padding: 3px;
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
  overflow-wrap: break-word;
  word-wrap: break-word;
  word-break: break-word;
  -webkit-line-break: anywhere;
}

.job-item:hover {
  background: rgb(233, 233, 233);
}

.job-link:visited {
  color: rgb(116, 116, 116);
}
</style>
