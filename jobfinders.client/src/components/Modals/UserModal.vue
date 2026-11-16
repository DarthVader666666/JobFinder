<script setup>
import Dialog from "primevue/dialog";
import Button from "primevue/button";

const emit = defineEmits(["handleLogOut"]);
</script>
<template>
  <Dialog modal :draggable="false">
    <template #header></template>
    <Button @click="emit('handleLogOut')">Выйти</Button>
  </Dialog>
</template>
