<script setup>
  import InputText from 'primevue/inputtext';
  import Button from 'primevue/button'

  const props = defineProps({
    changeSpeciality: {
        type: Function,
        required: true
      },
      changeLocation: {
        type: Function,
        required: true
      },
      findJobs: {
        type: Function,
        required: true
      }
  })
</script>

<template>
  <div class="serch-bar">
    <InputText @keyup.enter="findJobs()" @input="props.changeSpeciality($event.target.value)" type="text" placeholder="Специальность / должность"/>
    <InputText @keyup.enter="findJobs()" @input="props.changeLocation($event.target.value)" type="text" placeholder="Город / локация"/>
    <Button class="find-btn" @click="findJobs()">Найти</button>
  </div>
</template>

<style scoped>
  .serch-bar {
    display: flex;
    flex-direction: column;
    gap: 10px;
    justify-items: start;
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
