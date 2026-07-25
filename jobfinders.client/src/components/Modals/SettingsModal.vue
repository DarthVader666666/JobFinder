<script setup>
import Dialog from "primevue/dialog";
import SourcesComponent from "../SourcesComponent.vue";
import FilterComponent from "../FilterComponent.vue";
import Button from "primevue/button";
import { useStore } from "vuex";

const store = useStore();

async function closeFilter() {
  store.commit("setShowSettingsModal", false);
}
</script>

<template>
  <Dialog
    style="width: 90%; padding: 20px 0 20px 0"
    :draggable="false"
    modal
    :showHeader="false"
    class="filter-dialog"
  >
    <div class="settings">
      <SourcesComponent />
      <hr />
      <FilterComponent />
    </div>

    <Button
      @click="closeFilter"
      class="ok-button"
      style="height: 65px; width: 65px"
      rounded
    >
      OK
    </Button>
  </Dialog>
</template>

<style scoped>
.filter-dialog {
  position: relative;
}

.filter-dialog .ok-button {
  position: absolute;
  bottom: -35px;
  left: 50%;
  transform: translateX(-50%);
  box-shadow: 0px 1px 6px rgba(0, 0, 0, 0.3);
}
</style>
