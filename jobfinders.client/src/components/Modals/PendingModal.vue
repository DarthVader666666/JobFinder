<script setup>
import Dialog from "primevue/dialog";
</script>
<template>
  <Dialog
    modal
    :draggable="false"
    :closable="false"
    style="
      background: transparent;
      border: 0;
      box-shadow: none;
      text-align: center;
    "
  >
    <div class="content">
      <i class="pi pi-spin pi-spinner-dotted"></i>
      <span>Поиск...</span>
    </div>
  </Dialog>
</template>
<style>
.pending-modal .p-dialog-content {
  overflow: hidden;
}

.content {
  display: flex;
  flex-direction: column;
  font-size: 2rem;
  color: lightblue;
  margin: 10px;
  padding: 10px;
  i {
    font-size: 10rem;
  }
}
</style>
