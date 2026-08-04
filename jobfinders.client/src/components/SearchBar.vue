<script setup>
import InputText from 'primevue/inputtext';
import Button from 'primevue/button'
import AutoComplete from 'primevue/autocomplete';
import { useStore } from 'vuex';
import { computed, ref } from 'vue';
import { useToast } from "primevue/usetoast";
import { searchbarHelper } from '@/searchbarHelper';

const toast = useToast();
const store = useStore()
const jobs = computed(() => store.getters.getFilteredJobs)

const filteredSpecialities = ref([]);

async function findJobs() {
  store.commit('setShowSearchBarModal', false)

  const response = await store.dispatch("downloadJobs");

  if (response.status === 500) {
    store.dispatch('showError', { toast: toast, summary: 'showError', detail: `Ошибка сервера: ${response.data.errorText}` });
  } else if (response.status === 200){
    store.dispatch('showSuccess', { toast: toast, summary: "OK", detail: `Найдено совпадений: ${jobs.value.length}` });
    store.dispatch('showSavedJobs', false)
    window.scrollTo({ top: 0, behavior: "smooth" });
  }
}

function searchSpecialities(event) {
    const query = event.query.toLowerCase();
    filteredSpecialities.value = query ? searchbarHelper.specialities.filter((item) => item.toLowerCase().includes(query)) : [...searchbarHelper.specialities];
};

</script>

<template>
    <form class="serch-bar" v-on:submit.prevent="findJobs">
      <AutoComplete
        v-model="store.state.jobsRequest.speciality"
        :suggestions="filteredSpecialities"
        @complete="searchSpecialities"
        placeholder="Специальность / должность / компания"
        required="true"
        scrollHeight="14rem"
        emptySearchMessage=""
      >

      </AutoComplete>


      <!-- <InputText v-model="store.state.jobsRequest.speciality" type="text" placeholder="Специальность / должность / компания" required="true"/> -->
      <InputText v-model="store.state.jobsRequest.location" type="text" placeholder="Город / локация"/>
      <div style="text-align: end;">
        <Button class="find-btn" type="submit">Найти</button>
      </div>
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

  .p-autocomplete:deep(input) {
    width: 100%;
  }
</style>
