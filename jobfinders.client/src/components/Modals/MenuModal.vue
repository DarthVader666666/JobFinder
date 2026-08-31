<script setup>
import Dialog from "primevue/dialog";
import Button from "primevue/button";
import InputText from "primevue/inputtext";
import { ref } from "vue";
import { useStore } from "vuex";

const store = useStore();
const email = ref("");

function logInHandler() {
  store.dispatch("sendCode", email.value);
}
</script>
<template>
  <Dialog modal style="max-width: 90%; min-width: 35px">
    <template #header>
      <div>
        <span style="font-size: 1.2em; width: 90%">Вход</span>
      </div>
    </template>
    <div class="menu">
      <div style="display: flex; flex-direction: column; align-items: start">
        <span>Ваш Email</span>
        <InputText v-model="email" placeholder="Email"></InputText>
      </div>
      <Button raised @click="logInHandler">OK</Button>
    </div>
  </Dialog>
</template>
<style scoped>
.menu {
  display: flex;
  flex-direction: column;
  text-align: center;
  gap: 25px;
  margin: 20px;
}
</style>
