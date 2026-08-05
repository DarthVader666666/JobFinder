<script setup>
import Button from 'primevue/button'
import AutoComplete from 'primevue/autocomplete';
import { useStore } from 'vuex';
import { computed, ref } from 'vue';
import { useToast } from "primevue/usetoast";
import { searchbarHelper } from '@/searchbarHelper';

const toast = useToast();
const store = useStore()

const emit = defineEmits(['resetFirstPage'])

const jobs = computed(() => store.getters.getFilteredJobs)

const filteredSpecialities = ref([]);
const filteredLocations = ref([]);

async function findJobs() {
  store.commit('setShowSearchBarModal', false)

  const response = await store.dispatch("downloadJobs");
  emit('resetFirstPage')

  if (response.status === 500) {
    store.dispatch('showError', { toast: toast, summary: 'showError', detail: `Ошибка сервера: ${response.data.errorText}` });
  } else if (response.status === 200){
    store.dispatch('showSuccess', { toast: toast, summary: "OK", detail: `Найдено совпадений: ${jobs.value.length}` });
    store.dispatch('showSavedJobs', false)
    window.scrollTo({ top: 0, behavior: "smooth" });
  }
}

function searchSpeciality(event) {
    const query = event.query.toLowerCase();
    filteredSpecialities.value = query
      ? searchbarHelper.specialities.filter((item) => item.toLowerCase().includes(query))
      : [...searchbarHelper.specialities];
};

function searchLocation(event) {
    const query = event.query.toLowerCase();
    filteredLocations.value = query
      ? searchbarHelper.locations.filter((item) => item.toLowerCase().includes(query))
      : [...searchbarHelper.locations];
};

</script>

<template>
    <form class="serch-bar" v-on:submit.prevent="findJobs">
      <AutoComplete
        v-model="store.state.jobsRequest.speciality"
        :suggestions="filteredSpecialities"
        @complete="searchSpeciality"
        placeholder="Специальность / должность / компания"
        required="true"
        scrollHeight="14rem"
      >
      </AutoComplete>
      <AutoComplete
        v-model="store.state.jobsRequest.location"
        :suggestions="filteredLocations"
        @complete="searchLocation"
        placeholder="Город / локация"
        scrollHeight="14rem"
      >
      </AutoComplete>

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
