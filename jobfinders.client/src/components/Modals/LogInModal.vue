<script setup>
import Dialog from "primevue/dialog";
import Button from "primevue/button";
import InputText from "primevue/inputtext";
import ToggleSwitch from "primevue/toggleswitch";
import { computed, ref } from "vue";
import { useStore } from "vuex";

const store = useStore();

const emit = defineEmits(["showLogInModalHandler"]);

const email = ref("");
const password = ref("");
const repeatPassword = ref("");
const registerMode = ref(false);
const usePassword = ref(false);

const passwordsMatch = computed(
  () => password.value.length > 0 && password.value === repeatPassword.value,
);

const okDisabled = computed(() =>
  registerMode.value
    ? !(email.value.length > 0 && passwordsMatch.value)
    : email.value.length === 0,
);

const repeatPasswordInvalid = computed(() =>
  repeatPassword.value.length === 0 ? false : !passwordsMatch.value,
);

function logInHandler() {
  if (!usePassword.value) {
    store.dispatch("sendCode", email.value);
  }

  emit("showLogInModalHandler", false);
}

function registerHandler() {
  emit("showLogInModalHandler", false);
}

function resetInputs() {
  ((email.value = ""), (password.value = ""), (repeatPassword.value = ""));
}
</script>
<template>
  <Dialog
    modal
    @hide="
      () => {
        resetInputs();
        registerMode = false;
        usePassword = false;
      }
    "
    :draggable="false"
    style="max-width: 90%; min-width: 330px; height: 430px"
  >
    <template #header>
      <span style="font-size: 1.1rem">Вход / Регистрация</span>
    </template>
    <div class="menu-toggle">
      <span :style="!registerMode ? { fontWeight: 'bold' } : {}">Вход</span>
      <ToggleSwitch v-model="registerMode" @change="resetInputs"></ToggleSwitch>
      <span :style="registerMode ? { fontWeight: 'bold' } : {}"
        >Регистрация</span
      >
    </div>
    <form
      class="login-form"
      id="login-form"
      @submit.prevent="registerMode ? registerHandler() : logInHandler()"
    >
      <div v-if="!registerMode">
        <div v-if="usePassword" class="menu">
          <Button
            icon="pi pi-arrow-left"
            rounded
            raised
            severity="contrast"
            @click="() => (usePassword = false)"
          ></Button>
          <div>
            <span style="font-size: 0.9rem">Email</span>
            <InputText
              v-model="email"
              placeholder="Email"
              type="email"
              required
            ></InputText>
          </div>
          <div>
            <span style="font-size: 0.9rem">Пароль</span>
            <InputText
              v-model="password"
              placeholder="Пароль"
              type="password"
              required
            ></InputText>
          </div>
        </div>
        <div v-else>
          <span style="font-size: 0.9rem">Email</span>
          <InputText
            v-model="email"
            placeholder="Email"
            type="email"
            required
          ></InputText>
        </div>
      </div>
      <div v-else class="menu">
        <div>
          <span style="font-size: 0.9rem; text-align: start">Email</span>
          <InputText
            v-model="email"
            placeholder="Email"
            type="email"
            required
          ></InputText>
        </div>
        <div>
          <span style="font-size: 0.9rem">Пароль</span>
          <InputText
            v-model="password"
            placeholder="Пароль"
            type="password"
            required
          ></InputText>
        </div>
        <div>
          <span style="font-size: 0.9rem">Подтвердите пароль</span>
          <InputText
            v-model="repeatPassword"
            placeholder="Подтвердите пароль"
            type="password"
            :invalid="repeatPasswordInvalid"
          ></InputText>
        </div>
      </div>
      <div class="menu-buttons">
        <Button raised :disabled="okDisabled" type="submit" form="login-form"
          >OK</Button
        >
        <Button
          raised
          severity="secondary"
          @click="emit('showLogInModalHandler', false)"
          >Отмена</Button
        >
      </div>
      <div v-if="!usePassword && !registerMode">
        <Button severity="contrast" raised @click="() => (usePassword = true)"
          >Войти с паролем</Button
        >
      </div>
    </form>
  </Dialog>
</template>
<style scoped>
.menu {
  gap: 10px;
}

.login-form {
  display: flex;
  flex-direction: column;
  margin: 20px;

  div {
    display: flex;
    flex-direction: column;
  }
}

.menu-toggle {
  display: flex;
  gap: 10px;
  align-items: center;
  justify-content: center;
}

.menu-buttons {
  display: flex;
  flex-direction: row !important;
  justify-content: space-between;
  padding-top: 15px;
  padding-bottom: 15px;
  .p-button {
    width: 100px;
  }
}
</style>
