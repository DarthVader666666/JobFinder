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

const speciality = computed({ get: () => store.getters.getSpeciality, set: (value) => store.commit('setSpeciality', value) })
const location = computed({ get: () => store.getters.getLocation, set: (value) => store.commit('setLocation', value) })

async function findJobs() {
  if(!(speciality.value && store.state.finders.some(x => x.active))) {
    return
  }

  store.commit('setShowSearchBarModal', false)

  const response = await store.dispatch("downloadJobs");
  emit('resetFirstPage')

  if (response.status === 500) {
    store.dispatch('showError', { toast: toast, summary: 'Ошибка сервера', detail: `${response.error}` });
  } else if (response.status === 200){
    store.dispatch('showSuccess', { toast: toast, summary: "OK", detail: `Найдено совпадений: ${jobs.value.length}` });
    window.scrollTo({ top: 0, behavior: "smooth" });
  } else if (response.status === 499) {
    store.dispatch('showInfo', { toast: toast, summary: "Поиск прерван", detail: 'Отменено пользователем' });
  }
}

function searchSpeciality(event) {
    const query = event.query.toLowerCase();
    filteredSpecialities.value = query
      ? searchbarHelper.specialities.filter((item) => item.toLowerCase().startsWith(query))
      : [...searchbarHelper.specialities];
};

function searchLocation(event) {
    const query = event.query.toLowerCase();
    filteredLocations.value = query
      ? searchbarHelper.locations.filter((item) => item.toLowerCase().startsWith(query))
      : [...searchbarHelper.locations];
};

</script>

<template>
    <form class="serch-bar" v-on:submit.prevent="findJobs">
      <AutoComplete
        v-model="speciality"
        :suggestions="filteredSpecialities"
        @complete="searchSpeciality"
        placeholder="Специальность / должность / компания"
        required="true"
        scrollHeight="14rem"
      >
      </AutoComplete>
      <AutoComplete
        v-model="location"
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
