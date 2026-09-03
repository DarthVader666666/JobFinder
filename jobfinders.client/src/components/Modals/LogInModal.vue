<script setup>
import Dialog from "primevue/dialog";
import Button from "primevue/button";
import InputText from "primevue/inputtext";
import ToggleSwitch from "primevue/toggleswitch";
import InputOtp from "primevue/inputotp";
import { computed, ref, watch } from "vue";
import { useStore } from "vuex";

const props = defineProps({
  toast: {
    type: Object,
  },
});

const store = useStore();

const emit = defineEmits(["showLogInModalHandler"]);

const email = ref("");
const password = ref("");
const repeatPassword = ref("");
const code = ref("");
const registerMode = ref(false);
const usePassword = ref(false);
const showOtp = ref(false);
const time = ref(60);
const codeInvalid = ref(false);
let timerInterval = null;

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

watch(code, async (newValue) => {
  if (newValue.length === 4) {
    const response = await store.dispatch("signInWithCode", {
      email: email.value,
      code: newValue,
      toast: props.toast,
    });

    if (response.status != 200) {
      codeInvalid.value = true;
    } else {
      emit("showLogInModalHandler", false);
    }
  }
});

function signInHandler() {
  if (!usePassword.value) {
    const responsePropmise = store.dispatch("sendCode", {
      email: email.value,
      toast: props.toast,
    });
    waitForResponse(responsePropmise);

    showOtp.value = true;
    startTimer();
  } else {
    store.dispatch("signInWithPassword", {
      email: email.value,
      password: password.value,
      toast: props.toast,
    });
    emit("showLogInModalHandler", false);
  }
}

async function waitForResponse(responsePropmise) {
  const response = await responsePropmise;

  if (response.status === 500 || response.status === 400) {
    resetInputs();
  }
}

async function signUpHandler() {
  showOtp.value = true;
  startTimer();
  const response = await store.dispatch("signUp", {
    email: email.value,
    password: password.value,
    toast: props.toast,
  });

  if (response.status != 200) {
    backToForm();
  }
}

function resendCode() {
  code.value = "";
  store.dispatch("sendCode", { email: email.value, toast: props.toast });
  startTimer();
}

function resetInputs() {
  ((email.value = ""),
    (password.value = ""),
    (repeatPassword.value = ""),
    (showOtp.value = false),
    (code.value = ""));
}

function backToForm() {
  showOtp.value = false;
  code.value = "";
  timerInterval = null;
}

function startTimer() {
  time.value = 60;

  if (timerInterval) {
    clearInterval(timerInterval);
    timerInterval = null;
  }

  timerInterval = setInterval(() => {
    time.value--;

    if (time.value <= 0) {
      time.value = 0;
      clearInterval(timerInterval);
      timerInterval = null;
    }
  }, 1000);
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
    <div v-if="showOtp" class="otp">
      <span>Код подтверждения выслан на Email</span>
      <div class="timer">
        <Button
          icon="pi pi-arrow-left"
          rounded
          raised
          severity="contrast"
          @click="backToForm"
        ></Button>
        <span>{{ time }} с.</span>
      </div>
      <InputOtp v-model="code" :invalid="codeInvalid" integerOnly></InputOtp>
      <Button raised severity="contrast" @click="resendCode">Повторить</Button>
    </div>
    <div v-else>
      <div class="menu-toggle">
        <span :style="!registerMode ? { fontWeight: 'bold' } : {}">Вход</span>
        <ToggleSwitch
          v-model="registerMode"
          @change="resetInputs"
        ></ToggleSwitch>
        <span :style="registerMode ? { fontWeight: 'bold' } : {}"
          >Регистрация</span
        >
      </div>
      <form
        class="login-form"
        id="login-form"
        @submit.prevent="registerMode ? signUpHandler() : signInHandler()"
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
    </div>
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

.otp {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 30px;
}

.timer {
  display: flex;
  align-items: center;
  width: 100%;
  justify-content: space-around;
}
</style>
