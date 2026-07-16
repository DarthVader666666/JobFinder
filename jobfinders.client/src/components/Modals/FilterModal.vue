<script setup>
import Dialog from "primevue/dialog";
import SourcesComponent from "../SourcesComponent.vue";
</script>

<template>
  <Dialog style="width: 90%" :draggable="false" modal>
    <template #header>
      <span style="width: 90%"></span>
    </template>
    <SourcesComponent />
  </Dialog>
</template>

<style></style>
