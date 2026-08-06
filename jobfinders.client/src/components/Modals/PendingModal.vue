<script setup>
import Button from "primevue/button";
import Dialog from "primevue/dialog";
import { useStore } from "vuex";

const store = useStore();

function cancelDownload() {
  if (store.state.abortController) {
    store.state.abortController.abort();
  }
}
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
      <Button severity="secondary" @click="cancelDownload">Отмена</Button>
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
  gap: 20px;
  font-size: 2rem;
  color: lightblue;
  margin: 10px;
  padding: 10px;
  i {
    font-size: 10rem;
  }
  .p-button {
    background: transparent;
    color: lightblue;
    font-size: 1.4rem;
    /* text-shadow: 2px 1px 2px rgba(0, 0, 0, 0.6); */
    border: 0;
  }
}
</style>
