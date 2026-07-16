<script setup>
import Dialog from "primevue/dialog";
import SourcesComponent from "../SourcesComponent.vue";
import FilterComponent from "../FilterComponent.vue";
import Button from "primevue/button";
import { useStore } from "vuex";
import { computed } from "vue";

const store = useStore();
const emit = defineEmits(["showError", "showSuccess"]);

const jobs = computed(() => store.getters.getJobs);

async function implementFilter() {
  store.commit("setShowSettingsModal", false);

  const bodyValue = store.getters.getJobsRequest;
  const response = await store.dispatch("downloadJobs", bodyValue);

  if (response.status === 500) {
    emit("showError", "Ошибка сервера", response.data);
  } else {
    emit("showSuccess", "OK", `Найдено совпадений: ${jobs.value.length}`);
  }
}
</script>

<template>
  <Dialog style="width: 90%" :draggable="false" modal>
    <template #header>
      <span style="width: 90%"></span>
    </template>
    <SourcesComponent></SourcesComponent>
    <hr />
    <FilterComponent></FilterComponent>
    <hr />
    <Button @click="implementFilter">Применить</Button>
  </Dialog>
</template>

<style></style>
