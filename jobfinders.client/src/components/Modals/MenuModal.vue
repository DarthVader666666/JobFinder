<script setup>
import Dialog from "primevue/dialog";
import Button from "primevue/button";
</script>
<template>
  <Dialog modal style="max-width: 90%">
    <template #header>
      <div>
        <span style="font-size: 1.4em; width: 90%">Меню</span>
      </div>
    </template>
    <div class="menu">
      <Button severity="secondary" raised>Войти</Button>
      <Button severity="secondary" raised>Зарегестрироваться</Button>
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
