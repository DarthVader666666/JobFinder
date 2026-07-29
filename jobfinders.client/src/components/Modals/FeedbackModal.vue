<script setup>
import Dialog from "primevue/dialog";
import Textarea from "primevue/textarea";
import Button from "primevue/button";
import { ref } from "vue";
import { useStore } from "vuex";

const props = defineProps({
  toast: {
    type: Object,
  },
});

const store = useStore();
const comment = ref("");
const emit = defineEmits(["handleShowFeedbackModal"]);

function sendComment() {
  if (comment.value) {
    store.dispatch("sendComment", {
      toast: props.toast,
      request: { comment: comment.value },
    });
  }

  handleClose(false);
}

function handleClose(visible) {
  emit("handleShowFeedbackModal", false);
  if (!visible) {
    comment.value = "";
  }
}
</script>
<template>
  <Dialog
    style="width: 90%; max-width: 800px; height: 40%; min-height: 300px"
    modal
    :draggable="false"
    @update:visible="handleClose"
  >
    <template #header>
      <span style="width: 90%; font-size: 1.2rem; text-align: center"
        >Оставьте ваш отзыв или предложение по сайту</span
      >
    </template>
    <div
      style="
        display: flex;
        flex-direction: column;
        gap: 15px;
        align-items: end;
        height: 100%;
      "
    >
      <Textarea
        v-model="comment"
        style="width: 100%; height: 80%; resize: none"
        required
        placeholder="Ваш комментарий"
      ></Textarea>
      <Button style="height: 40px; width: 120px" @click="sendComment"
        >OK</Button
      >
    </div>
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
