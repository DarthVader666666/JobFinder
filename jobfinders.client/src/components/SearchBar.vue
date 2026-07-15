<script setup>
import InputText from 'primevue/inputtext';
import Button from 'primevue/button'

const emit = defineEmits(["changeSpeciality", "changeLocation", "findJobs"])

</script>

<template>
  <div class="serch-bar">
    <InputText @keyup.enter="emit('findJobs')" @input="emit('changeSpeciality', $event.target.value)" type="text" placeholder="Специальность / должность"/>
    <InputText @keyup.enter="emit('findJobs')" @input="emit('changeLocation', $event.target.value)" type="text" placeholder="Город / локация"/>
    <Button class="find-btn" @click="emit('findJobs')">Найти</button>
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
