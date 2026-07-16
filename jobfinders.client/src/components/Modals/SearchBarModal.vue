<script setup>
import SearchBar from "../SearchBar.vue";
import Dialog from "primevue/dialog";
</script>
<template>
  <Dialog style="width: 90%" modal :draggable="false">
    <template #header>
      <span style="width: 90%"></span>
    </template>
    <SearchBar></SearchBar>
  </Dialog>
</template>
<style>
.p-dialog-header {
  padding: 10px 10px 15px 15px;
  .p-button {
    background: rgb(210, 210, 210);
  }

  svg {
    height: 1.5rem;
    width: 1.5rem;
  }
}
</style>
