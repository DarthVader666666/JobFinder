<script setup>
import Dialog from "primevue/dialog";
import SourcesComponent from "../SourcesComponent.vue";
import FilterComponent from "../FilterComponent.vue";
import Button from "primevue/button";
import { useStore } from "vuex";
import { computed } from "vue";
import { useToast } from "primevue/usetoast";

const store = useStore();
const toast = useToast();
const jobs = computed(() => store.getters.getFilteredJobs);

async function implementFilter() {
  store.commit("setShowSettingsModal", false);

  const bodyValue = store.getters.getJobsRequest;
  const response = await store.dispatch("downloadJobs", bodyValue);

  if (response.status === 500) {
    store.dispatch("showError", {
      toast: toast,
      summary: "showError",
      detail: `Ошибка сервера: ${response.data.errorText}`,
    });
  } else if (response.status === 200) {
    store.dispatch("showSuccess", {
      toast: toast,
      summary: "OK",
      detail: `Найдено совпадений: ${jobs.value.length}`,
    });
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
