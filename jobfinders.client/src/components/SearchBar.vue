<script setup>
import InputText from 'primevue/inputtext';
import Button from 'primevue/button'
import { useStore } from 'vuex';
import { computed } from 'vue';

const store = useStore()
const emit = defineEmits(["showError", "showSuccess"])

const jobs = computed(() => store.getters.getJobs)

async function findJobs() {
  store.commit('setShowSearchBarModal', false)

  const bodyValue = store.getters.getJobsRequest;
  const response = await store.dispatch("downloadJobs", bodyValue);

  if (response.status === 500) {
    emit('showError', "Ошибка сервера", response.data);
  } else {
    emit('showSuccess', "OK", `Найдено совпадений: ${jobs.value.length}`);
  }
}

</script>

<template>
    <form class="serch-bar" v-on:submit.prevent="findJobs">
      <InputText v-model="store.state.jobsRequest.speciality" type="text" placeholder="Специальность / должность" required="true"/>
      <InputText v-model="store.state.jobsRequest.location" type="text" placeholder="Город / локация"/>
      <Button class="find-btn" type="submit">Найти</button>
    </form>
</template>

<style scoped>
  .serch-bar {
    display: flex;
    flex-direction: column;
    gap: 10px;
    margin-bottom: 10px;
  }

  .find-btn {
    font-weight: bold;
    width: 50%;
  }

  .find-btn:hover {
    cursor: pointer;
  }
</style>
